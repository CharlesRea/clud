using System;
using System.Linq;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using KubeClient;
using Microsoft.EntityFrameworkCore;

namespace Clud.Api.Services
{
    public class ApplicationService : Applications.ApplicationsBase
    {
        private readonly DataContext dataContext;
        private readonly KubeApiClient kubeApiClient;

        public ApplicationService(DataContext dataContext, KubeApiClient kubeApiClient)
        {
            this.dataContext = dataContext;
            this.kubeApiClient = kubeApiClient;
        }

        public override async Task<ListApplicationsResponse> ListApplications(ListApplicationsRequest request, ServerCallContext context)
        {
            var response = new ListApplicationsResponse();

            var applications = await dataContext.Applications.Select(a => new ListApplicationsResponse.Types.Application
            {
                Name = a.Name,
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
                Url = $"{application.Name}.clud" // TODO pull URL generation out somewhere,
            };
            response.Services.AddRange(application.Services.Select(ProjectToServiceResponse));

            return response;

            ApplicationResponse.Types.ServiceResponse ProjectToServiceResponse(Service service)
            {
                var serviceResponse = new ApplicationResponse.Types.ServiceResponse
                {
                    Name = service.Name,
                    Url = $"{service.Name}.{application.Name}.clud" // TODO pull URL generation out somewhere,
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
