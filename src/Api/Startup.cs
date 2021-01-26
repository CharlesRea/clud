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

namespace Clud.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CludOptions>(Configuration.GetSection("Clud"));

            services.AddMvcCore();

            services.AddGrpc();

            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "client/build"; });

            services.AddCors(options =>
                options.AddPolicy("LocalhostPolicy", builder => builder
                    .WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                )
            );

            services.AddDbContext<DataContext>(
                options => options.UseNpgsql(
                    Configuration.GetValue<string>("Clud:ConnectionString"),
                    dbOptions => dbOptions.EnableRetryOnFailure(maxRetryCount: 2)
                ).UseSnakeCaseNamingConvention()
            );

            services.AddKubeClient(GetKubeClientOptions());
        }

        private static KubeClientOptions GetKubeClientOptions()
        {
            var options = string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST"))
                ? K8sConfig.Load().ToKubeClientOptions()
                : KubeClientOptions.FromPodServiceAccount();
            options.KubeNamespace = "default";
            return options;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();

            app.UseCors("LocalhostPolicy");

            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<DeploymentsService>();
                endpoints.MapGrpcService<ApplicationService>();
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "client";

                if (env.IsDevelopment())
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                }
            });
        }
    }
}
