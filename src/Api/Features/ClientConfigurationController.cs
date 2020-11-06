using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Clud.Api.Features
{
    [ApiController]
    [Route("api/config")]
    public class ClientConfigurationController : ControllerBase
    {
        private readonly CludOptions cludOptions;

        public ClientConfigurationController(IOptions<CludOptions> cludOptions)
        {
            this.cludOptions = cludOptions.Value;
        }

        [HttpGet]
        [AllowAnonymous]
        public Dictionary<string, object> GetClientConfiguration()
        {
            return new Dictionary<string, object>
            {
                {
                    "Clud",
                    new Dictionary<string, object>
                    {
                        { "BaseHostname", cludOptions.BaseHostname }
                    }
                }
            };
        }
    }
}
