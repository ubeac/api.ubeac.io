using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using uBeac.Idsrv.Models;
using uBeac.Idsrv.Services;

namespace uBeac.Idsrv
{
    public static class IdentityServerExtensions
    {
        public static IServiceCollection AddIdentityServerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                 .AddIdentityServerAuthentication(options =>
                 {
                     options.Authority = configuration.GetValue<string>("IdsrvAuthority");
                     options.ApiName = "uBeacIdsrvApi";
                     options.RequireHttpsMetadata = false;
                 });

            var setupAction = new Action<IdentityOptions>(options =>
            {
                //todo: Amir, what is this for?
                options.Tokens.ChangePhoneNumberTokenProvider = "Phone";
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1440);
            });

            services.AddIdentity<User, Role>(setupAction)
             .AddUserStore<UserStore>()
             .AddDefaultTokenProviders();

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            });

            builder.AddAspNetIdentity<User>();
            builder.AddInMemoryIdentityResources(configuration.GetSection("IdentityResources"));
            builder.AddInMemoryApiResources(configuration.GetSection("ApiResources"));
            builder.AddInMemoryClients(configuration.GetSection("Clients"));
            builder.AddDeveloperSigningCredential();
            builder.AddProfileService<ProfileService>();
            return services;
        }

    }
}
