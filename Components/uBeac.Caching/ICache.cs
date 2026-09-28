using System;
using System.Threading.Tasks;

namespace uBeac.Caching
{
    public interface ICache
    {
        Task<TItem> GetOrCreateAsync<TItem>(object key, Func<Task<TItem>> factory);
        bool TryGetValue<TItem>(object key, out TItem result);
        TItem Set<TItem>(object key, TItem item);
    }   
}
