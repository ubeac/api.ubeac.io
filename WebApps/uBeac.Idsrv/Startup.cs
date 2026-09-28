using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using uBeac.Idsrv.Models;
using uBeac.Idsrv.Repositories;
using uBeac.Idsrv.Repositories.MongoDB;
using uBeac.Idsrv.Services;
using uBeac.Repositories.MongoDB;
using uBeac.Web.Middlewares;

namespace uBeac.Idsrv
{
    public class Startup
    {
        public IHostingEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddIdentityServerConfiguration(Configuration);

            services.AddMongoDBLog();

            services.AddMongo<MainDatabase>("uBeacDBConnection");
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();

            services.AddScoped<IUserStore<User>, UserStore>();
            services.AddScoped<IRoleStore<Role>, RoleStore>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IEmailService, EmailService>();

            services.AddResponseCaching();

            services.AddLocalFileStorages();

            //// mail server config
            services.AddMailServer();

            // swagger config
            services.AddApiDocumantation(Configuration);

            // CORS config
            services.AddCustomCors(Configuration);

            services.AddResponseCompression();

            // adding mvc
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            
            app.UseCustomCors();
            app.UseIdentityServer();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<LoggingMiddleware>();
            app.UseResponseCompression();
            app.UseResponseCaching();
            app.UseAuthentication();
            app.UseApiDocumantation(Environment);
            app.UseMvcWithDefaultRoute();
        }       
    }
}
