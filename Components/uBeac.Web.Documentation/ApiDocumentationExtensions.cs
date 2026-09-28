/*
 * ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------
*/

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using uBeac;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiDocumentationExtensions
    {

        public static IServiceCollection AddApiDocumantation(this IServiceCollection services, IConfiguration configuration)
        {
            var appConfig = configuration.GetSection("AppConfig").Get<AppConfig>();
            services.TryAddSingleton(appConfig);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(appConfig.Version, new Info { Title = appConfig.Name, Version = appConfig.Version });
                c.EnableAnnotations();
            });

            return services;
        }

        public static IApplicationBuilder UseApiDocumantation(this IApplicationBuilder app, IHostingEnvironment env)
        {
            var appConfig = app.ApplicationServices.GetService<AppConfig>();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "doc";
                c.SwaggerEndpoint("/swagger/" + appConfig.Version + "/swagger.json", appConfig.Name);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.IndexStream = () => new FileStream($"{env.WebRootPath}/swagger.html", FileMode.Open);
            });

            return app;

        }
    }
}
