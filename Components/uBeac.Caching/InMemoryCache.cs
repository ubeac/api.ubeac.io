using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace uBeac.Caching
{
    public class InMemoryCache : ICache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly double _absoluteExpiration;

        public InMemoryCache(IConfiguration configuration)
        {
            var absoluteExpiration = double.Parse(configuration.GetSection(AbsoluteExpirationConfigSection).Value);
            double expirationScanFrequency = 1000;

            _memoryCache = new MemoryCache(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMilliseconds(expirationScanFrequency)
            });

            _absoluteExpiration = absoluteExpiration;
        }

        public async Task<TItem> GetOrCreateAsync<TItem>(object key, Func<Task<TItem>> factory)
        {
            if (!_memoryCache.TryGetValue(key, out object result))
            {
                var entry = _memoryCache.CreateEntry(key);
                entry.SetAbsoluteExpiration(TimeSpan.FromMilliseconds(_absoluteExpiration));
                result = await factory();
                entry.SetValue(result);
                entry.Dispose();
            }

            return (TItem)result;

        }

        public bool TryGetValue<TItem>(object key, out TItem result)
        {
            return _memoryCache.TryGetValue(key, out result);
        }

        public TItem Set<TItem>(object key, TItem item)
        {
            return _memoryCache.Set(key, item, TimeSpan.FromMilliseconds(_absoluteExpiration));
        }

        public virtual string AbsoluteExpirationConfigSection { get => "InMemoryAbsoluteExpiration"; }
    }
}
