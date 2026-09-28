/* ------------------------------------------------------------------
 * This is stable version, audit by by Amir in 2018-10-31 
 * ------------------------------------------------------------------*/

using Microsoft.Extensions.DependencyInjection.Extensions;
using uBeac.Services.Email;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MailServiceExtensions
    {
        public static IServiceCollection AddMailServer(this IServiceCollection services)
        {
            services.TryAddSingleton<IEmailSender, EmailSender>();
            return services;
        }
    }
}
