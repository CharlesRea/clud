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
using Microsoft.Extensions.Options;
using Shared;

namespace Clud.Api.Features
{
    public class ApplicationService : Applications.ApplicationsBase
    {
        private readonly DataContext dataContext;
        private readonly KubeApiClient kubeApiClient;
        private readonly CludOptions cludOptions;

        public ApplicationService(
            DataContext dataContext,
            KubeApiClient kubeApiClient,
            IOptions<CludOptions> cludOptions
        )
        {
            this.dataContext = dataContext;
            this.kubeApiClient = kubeApiClient;
            this.cludOptions = cludOptions.Value;
        }

        public override async Task<ListApplicationsResponse> ListApplications(ListApplicationsQuery request, ServerCallContext context)
        {
            return new ListApplicationsResponse
            {
                Applications =
                {
                    await dataContext.Applications.Select(a => new ListApplicationsResponse.Types.Application
                    {
                        Name = a.Name,
                        Description = a.Description,
                        Owner = a.Owner,
                        LastUpdatedTime = Timestamp.FromDateTimeOffset(a.UpdatedDateTime),
                    }).ToListAsync()
                }
            };
        }

        public override async Task<ApplicationResponse> GetApplication(ApplicationQuery request, ServerCallContext context)
        {
            var application = await dataContext.Applications.SingleOrThrowNotFound(a => a.Name == request.Name);

            var appDeployments = await dataContext.Deployments
                .Where(a => a.ApplicationId == application.ApplicationId)
                .OrderByDescending(d => d.DeploymentDateTime)
                .ToListAsync();

            var pods = (await kubeApiClient.PodsV1().List(kubeNamespace: application.Namespace))
                .ToLookup(pod => pod.Metadata.Labels[KubeNaming.AppLabelKey]);
            var services = (await kubeApiClient.ServicesV1().List(kubeNamespace: application.Namespace))
                .ToDictionary(s => s.Metadata.Name);
            var deployments = (await kubeApiClient.DeploymentsV1().List(kubeNamespace: application.Namespace))
                .ToDictionary(d => d.Metadata.Name);
            var statefulSets = (await kubeApiClient.StatefulSetV1().List(kubeNamespace: application.Namespace))
                .ToDictionary(d => d.Metadata.Name);
            var ingresses = (await kubeApiClient.IngressesV1Beta1().List(kubeNamespace: application.Namespace))
                .ToDictionary(d => d.Metadata.Name);

            var entryPointIngress = ingresses.GetValueOrDefault(KubeNaming.EntryPointIngressName);

            return new ApplicationResponse
            {
                Name = application.Name,
                IngressUrl = entryPointIngress != null ? "https://" + entryPointIngress.Spec.Rules.Single().Host : null,
                Description = application.Description,
                Owner = application.Owner,
                Repository = application.Repository,
                LastUpdatedTime = Timestamp.FromDateTimeOffset(application.UpdatedDateTime),
                Services = { services.Values.Select(ProjectToServiceResponse) },
                Deployments = { appDeployments.Select(ProjectToDeployment) },
            };

            ApplicationResponse.Types.Service ProjectToServiceResponse(ServiceV1 service)
            {
                var serviceName = service.Metadata.Name;
                var servicePods = pods[serviceName];
                var deployment = deployments.GetValueOrDefault(serviceName);
                var statefulSet = statefulSets.GetValueOrDefault(serviceName);
                var ingress = ingresses.GetValueOrDefault(serviceName);

                var podSpec = deployment?.Spec.Template.Spec
                              ?? statefulSet?.Spec.Template.Spec
                              ?? throw new InvalidOperationException($"No deployment or statefulSet for service {serviceName}");

                var podMetrics = deployment != null
                    ? new ApplicationResponse.Types.PodMetrics
                    {
                        TotalPods = deployment.Status.Replicas ?? 0,
                        UpToDatePods = deployment.Status.UpdatedReplicas ?? 0,
                        DesiredPods = deployment.Spec.Replicas ?? 1,
                        ReadyPods = deployment.Status.ReadyReplicas ?? 0,
                    }
                    : new ApplicationResponse.Types.PodMetrics
                    {
                        TotalPods = statefulSet.Status.Replicas,
                        UpToDatePods = statefulSet.Status.UpdatedReplicas ?? 0,
                        DesiredPods = statefulSet.Spec.Replicas ?? 1,
                        ReadyPods = statefulSet.Status.ReadyReplicas ?? 0,
                    };


                return new ApplicationResponse.Types.Service
                {
                    Name = serviceName,
                    InternalHostname = $"{serviceName}.{application.Name}",
                    IngressUrl = ingress != null
                        ? "https://" + ingress.Spec.Rules.Single().Host
                        : null,
                    Ports =
                    {
                        service.Spec.Ports
                            .Where(port => port.Name != KubeNaming.HttpPortName)
                            .Select(port => new ApplicationResponse.Types.Port
                            {
                                TargetPort = port.TargetPort.Int32Value,
                                ExposedPort = port.NodePort,
                                HostName = $"{serviceName}.{cludOptions.BaseHostname}",
                                Type = port.Protocol,
                            })
                    },
                    ImageName = podSpec.Containers.FirstOrDefault()?.Image ?? string.Empty,
                    PodMetrics = podMetrics,
                    Pods =
                    {
                        servicePods.Select(pod => new ApplicationResponse.Types.Pod
                        {
                            Name = pod.Metadata.Name,
                            CreationDate = Timestamp.FromDateTime(pod.Metadata.CreationTimestamp ?? throw new InvalidOperationException("Pod does not have a creationTimestamp")),
                            Status = pod.Status.Phase,
                            StatusMessage = pod.Status.Message ?? string.Empty,
                            Image = pod.Spec.Containers.FirstOrDefault()?.Image ?? string.Empty,
                        })
                    }
                };
            }

            ApplicationResponse.Types.Deployment ProjectToDeployment(Deployment deployment)
            {
                return new ApplicationResponse.Types.Deployment
                {
                    Version = deployment.Version,
                    CommitHash = deployment.CommitHash,
                    DeploymentDate = deployment.DeploymentDateTime.ToTimestamp(),
                };
            }
        }

        public override async Task<ApplicationVersionResponse> GetVersion(ApplicationVersionQuery request, ServerCallContext context)
        {
            var application = await dataContext.Applications.SingleOrDefaultAsync(a => a.Name == request.ApplicationName);
            if (application == null)
            {
                return new ApplicationVersionResponse();
            }

            var deploymentVersion = await dataContext.Deployments
                .Where(d => d.ApplicationId == application.ApplicationId)
                .OrderByDescending(d => d.Version)
                .Select(d => d.Version)
                .FirstAsync();

            return new ApplicationVersionResponse { Version = deploymentVersion };
        }

        public override async Task<SecretsResponse> GetSecrets(SecretsQuery request, ServerCallContext context)
        {
            var application = await dataContext.Applications.SingleOrDefaultAsync(a => a.Name == request.ApplicationName);

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
