using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace CachedWebApi01.Services
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeTimeLive);

        Task<string> GetCachedResponseAsync(string cacheKey);
    }

    public class ResponseCacheService : IResponseCacheService
    {
        //private readonly IDistributedCache _distributedCache;

        public ResponseCacheService(IDistributedCache distributedCache)
        {
            DistributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public IDistributedCache DistributedCache { get; }

        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeTimeLive)
        {
            if (cacheKey == null)
            {
                return;
            }

            if (response == null)
            {
                return;
            }

            var serializedResponse = JsonConvert.SerializeObject(response);

            await DistributedCache.SetStringAsync(
                cacheKey, 
                serializedResponse, 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = timeTimeLive
                });
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var cachedResponse = 
                await 
                DistributedCache
                .GetStringAsync(cacheKey);

            return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
        }
    }
}
