using System;
using Clud.Api.Features;
using Clud.Api.Infrastructure.DataAccess;
using KubeClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

namespace Clud.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CludOptions>(Configuration.GetSection("Clud"));

            services.AddGrpc();
            services.AddGrpcWeb(o => o.GrpcWebEnabled = true);

            services.AddDbContext<DataContext>(
                options => options.UseNpgsql(
                    Configuration.GetValue<string>("Clud:ConnectionString"),
                    dbOptions => dbOptions.EnableRetryOnFailure(maxRetryCount: 2)
                ).UseSnakeCaseNamingConvention()
            );

            services.AddTransient<UrlGenerator>();

            var kubeClientOptions = K8sConfig.Load().ToKubeClientOptions();
            kubeClientOptions.KubeNamespace = "default";
            services.AddKubeClient(kubeClientOptions);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseGrpcWeb();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<DeploymentsService>();
                endpoints.MapGrpcService<ApplicationService>();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
