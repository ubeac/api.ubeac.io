using uBeac.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AccessControlExtension
    {
        public static IServiceCollection AddAccessControl(this IServiceCollection services)
        {
            services.AddScoped<ISecurityContext, SecurityContext>(); 
            //services.AddSingleton<IUserIdentity, UserIdentity>(); 
            services.AddSingleton<IAccessService, AccessService>();
            services.AddSingleton<IAccessRepository, AccessRepository>();

            return services;
        }
    }
}
