using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using KubeClient;
using KubeClient.Models;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Clud.Api.Features
{
    public class ApplicationService : Applications.ApplicationsBase
    {
        private readonly DataContext dataContext;
        private readonly KubeApiClient kubeApiClient;
        private readonly UrlGenerator urlGenerator;

        public ApplicationService(DataContext dataContext, KubeApiClient kubeApiClient, UrlGenerator urlGenerator)
        {
            this.dataContext = dataContext;
            this.kubeApiClient = kubeApiClient;
            this.urlGenerator = urlGenerator;
        }

        public override async Task<ListApplicationsResponse> ListApplications(ListApplicationsQuery request, ServerCallContext context)
        {
            var response = new ListApplicationsResponse();

            var applications = await dataContext.Applications.Select(a => new ListApplicationsResponse.Types.Application
            {
                Name = a.Name,
                Description = a.Description,
                Owner = a.Owner,
                LastUpdatedTime =  Timestamp.FromDateTimeOffset(a.UpdatedDateTime),
            }).ToListAsync();

            response.Applications.AddRange(applications);

            return response;
        }

        public override async Task<ApplicationResponse> GetApplication(ApplicationQuery request, ServerCallContext context)
        {
            var application = await dataContext.Applications
                .Include(a => a.Services)
                .SingleOrThrowNotFound(a => a.Name == request.Name);

            var historyEntries = await dataContext.ApplicationHistories
                .Where(a => a.ApplicationId == application.ApplicationId)
                .ToListAsync();

            var pods = (await kubeApiClient.PodsV1().List(kubeNamespace: application.Namespace))
                .Where(pod => pod.Metadata.Labels.ContainsKey(KubeNaming.AppLabelKey))
                .ToLookup(pod => pod.Metadata.Labels[KubeNaming.AppLabelKey]);
            var kubeServices = (await kubeApiClient.ServicesV1().List(kubeNamespace: application.Namespace))
                .ToDictionary(s => s.Metadata.Name);
            var kubeDeployments = (await kubeApiClient.DeploymentsV1().List(kubeNamespace: application.Namespace))
                .ToDictionary(d => d.Metadata.Name);
            var kubeStatefulSets = (await kubeApiClient.StatefulSetV1().List(kubeNamespace: application.Namespace))
                .ToDictionary(d => d.Metadata.Name);

            var response = new ApplicationResponse
            {
                Name = application.Name,
                Url = urlGenerator.GetApplicationUrl(application.Name),
                Description = application.Description,
                Owner = application.Owner,
                Repository = application.Repository,
                LastUpdatedTime = Timestamp.FromDateTimeOffset(application.UpdatedDateTime),
            };

            response.Services.AddRange(application.Services.Select(ProjectToServiceResponse));
            response.History.AddRange(historyEntries.Select(ProjectToHistoryResponse));

            return response;

            ApplicationResponse.Types.ServiceResponse ProjectToServiceResponse(Service service)
            {
                var servicePods = pods[service.Name];
                var kubeService = kubeServices.GetValueOrDefault(service.Name) ?? throw new InvalidOperationException($"Service {service.Name} does not exist in namespace {application.Namespace}");
                var kubeDeployment = kubeDeployments.GetValueOrDefault(service.Name);
                var kubeStatefulSet = kubeStatefulSets.GetValueOrDefault(service.Name);
                var port = kubeService.Spec.Ports.SingleOrDefault(p => p.Name == KubeNaming.HttpPortName);

                var serviceResponse = new ApplicationResponse.Types.ServiceResponse
                {
                    Name = service.Name,
                    ExternallyAccessible = true, // TODO set this properly
                    ExternalHostname = urlGenerator.GetExternalServiceHostname(application.Name, service.Name),
                    InternalHostname = urlGenerator.GetInternalServiceHostname(application.Name, service.Name, port?.Port ?? 0),
                    TotalPods = kubeDeployment?.Status.Replicas ?? 0,
                    UpToDatePods = kubeDeployment?.Status.UpdatedReplicas ?? 0,
                    DesiredPods = kubeDeployment?.Spec.Replicas ?? 1,
                    ReadyPods = kubeDeployment?.Status.ReadyReplicas ?? 0,
                    ImageName = kubeDeployment?.Spec.Template.Spec.Containers.FirstOrDefault()?.Image ?? string.Empty,
                };

                serviceResponse.Pods.AddRange(servicePods.Select(pod => new ApplicationResponse.Types.PodResponse
                {
                    Name = pod.Metadata.Name,
                    CreationDate = Timestamp.FromDateTime(pod.Metadata.CreationTimestamp ?? throw new InvalidOperationException("Pod does not have a creationTimestamp")),
                    Status = pod.Status.Phase,
                    StatusMessage = pod.Status.Message ?? string.Empty,
                    Image = pod.Spec.Containers.FirstOrDefault()?.Image ?? string.Empty,
                }));

                return serviceResponse;
            }

            ApplicationResponse.Types.ApplicationHistoryResponse ProjectToHistoryResponse(ApplicationHistory history)
            {
                return new ApplicationResponse.Types.ApplicationHistoryResponse
                {
                    Message = history.Message,
                    UpdateDate = history.UpdatedDateTime.ToTimestamp(),
                };
            }
        }

        public override async Task<SecretsResponse> GetSecrets(SecretsQuery request, ServerCallContext context)
        {
            var application = await dataContext.Applications
                .Include(a => a.Services)
                .SingleOrDefaultAsync(a => a.Name == request.ApplicationName);

            if (application == null)
            {
                return new SecretsResponse();
            }

            var secrets = await kubeApiClient.SecretsV1().List(kubeNamespace: application.Namespace);

            return new SecretsResponse
            {
                Services =
                {
                    secrets.Select(secret => new SecretsResponse.Types.Service
                    {
                        Name = secret.Metadata.Name,
                        Secrets =
                        {
                            secret.Data.Select(data => new SecretsResponse.Types.Secret { Name = data.Key })
                        }
                    })
                }
            };
        }
    }
}
