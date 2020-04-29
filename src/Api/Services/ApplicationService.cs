using System.Linq;
using System.Threading.Tasks;
using Clud.Api.Infrastructure.DataAccess;
using Clud.Grpc;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Clud.Api.Services
{
    public class ApplicationService : Applications.ApplicationsBase
    {
        private readonly DataContext dataContext;

        public ApplicationService(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public override async Task<ListApplicationsResponse> ListApplications(ListApplicationsRequest request, ServerCallContext context)
        {
            var response = new ListApplicationsResponse();

            var applications = await dataContext.Applications.Select(a => new ListApplicationsResponse.Types.Application()
            {
                ApplicationId = a.ApplicationId,
                Name = a.Name,
            }).ToListAsync();

            response.Applications.AddRange(applications);

            return response;
        }
    }
}
