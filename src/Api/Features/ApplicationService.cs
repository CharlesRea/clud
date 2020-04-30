using System;
using System.Linq;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using KubeClient;
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

        public override async Task<ListApplicationsResponse> ListApplications(ListApplicationsRequest request, ServerCallContext context)
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

        public override async Task<ApplicationResponse> GetApplication(GetApplicationRequest request, ServerCallContext context)
        {
            var application = await dataContext.Applications.Include(a => a.Services).SingleOrThrowNotFound(a => a.Name == request.Name);
            var pods = await kubeApiClient.PodsV1().List(kubeNamespace: application.Namespace);

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

            return response;

            ApplicationResponse.Types.ServiceResponse ProjectToServiceResponse(Service service)
            {
                var serviceResponse = new ApplicationResponse.Types.ServiceResponse
                {
                    Name = service.Name,
                    Url = urlGenerator.GetServiceUrl(application.Name, service.Name),
                };

                // TODO match pods to the correct service when we can deploy multiple services
                serviceResponse.Pods.AddRange(pods.Select(pod => new ApplicationResponse.Types.PodResponse
                {
                    Name = pod.Metadata.Name,
                    CreationDate = Timestamp.FromDateTime(pod.Metadata.CreationTimestamp ?? throw new InvalidOperationException("Pod does not have a creationTimestamp")),
                    Status = pod.Status.Phase,
                    StatusMessage = pod.Status.Message ?? string.Empty,
                }));

                return serviceResponse;
            }
        }
    }
}
