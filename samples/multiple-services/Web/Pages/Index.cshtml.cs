using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpClientFactory _clientFactory;

        public string Error { get; set; }

        public IEnumerable<WeatherForecast> Forecasts { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _clientFactory = clientFactory;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient();

                var response = await client.GetAsync("http://api:80/WeatherForecast");
                response.EnsureSuccessStatusCode();

                Forecasts = await JsonSerializer.DeserializeAsync<IEnumerable<WeatherForecast>>(
                    await response.Content.ReadAsStreamAsync(),
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
                );
            }
            catch (Exception exception)
            {
                Error = exception.Message;
            }
        }
    }

    public class WeatherForecast
    {
        public DateTime Date { get; set; }

        public int TemperatureC { get; set; }

        public int TemperatureF { get; set; }

        public string Summary { get; set; }
    }
}
