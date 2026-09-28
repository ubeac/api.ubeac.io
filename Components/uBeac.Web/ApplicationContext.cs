using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace uBeac.Web
{
    public class ApplicationContext : IApplicationContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationIdentity _identity;

        public ApplicationContext(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
            _identity = new ApplicationIdentity(httpContextAccessor);
        }

        public IApplicationIdentity User
        {
            get
            {
                return _identity;
            }
        }
    }
}
