using Microsoft.Extensions.Configuration;
using uBeac.Caching;

namespace uBeac.HttpHub.Caches
{
    public class ThrottleCache : InMemoryCache
    {
        public ThrottleCache(IConfiguration configuration) : base(configuration)
        {
        }
        public override string AbsoluteExpirationConfigSection => "TrottlingAbsoluteExpiration";
    }
}
