using System;
using System.Net.Http;
using System.Threading.Tasks;
using Clud.Grpc;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Clud.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.Configure<CludOptions>(options => builder.Configuration.Bind("Clud", options));

            builder.Services.AddSingleton(services =>
            {
                var httpClient = new HttpClient(new GrpcWebHandler(GrpcWebMode.GrpcWeb, new HttpClientHandler()));
                var baseUri = services.GetRequiredService<NavigationManager>().BaseUri;
                var channel = GrpcChannel.ForAddress(baseUri, new GrpcChannelOptions { HttpClient = httpClient });
                return new Applications.ApplicationsClient(channel);
            });

            await builder.Build().RunAsync();
        }

        private static async Task AddClientConfiguration(WebAssemblyHostBuilder builder)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            var configSettings = await httpClient.GetAsync("/api/config");
            configSettings.EnsureSuccessStatusCode();
            var configStream = await configSettings.Content.ReadAsStreamAsync();

            // The following is taken from Blazor's own implementation - see the implementation of WebAssemblyHostBuilder.CreateDefault:
            // Perf: Using this over AddJsonStream. This allows the linker to trim out the "File"-specific APIs and assemblies
            // for Configuration, of where there are several.
            builder.Configuration.Add<JsonStreamConfigurationSource>(s => s.Stream = configStream);
        }
    }
}
