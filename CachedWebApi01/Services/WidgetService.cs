using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CachedWebApi01.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace CachedWebApi01.Services
{
    public interface IWidgetService
    {
        Task<List<WidgetResponseItem>> Get(int id);
    }

    public class WidgetService : IWidgetService
    {
        [Cached(30 * CachedAttribute.OneSecond)]
        public async Task<List<WidgetResponseItem>> Get(int id)
        {
            // Emulate expensive work
            var startDateTime = DateTime.Now;

            var sleepMilliseconds = 5000;
            Thread.Sleep(sleepMilliseconds);

            var endDateTime = DateTime.Now;

            return new List<WidgetResponseItem> {
                new WidgetResponseItem {
                    Id = id,
                    Name = DateTime.Now.ToString(),
                    SleepMilliseconds = sleepMilliseconds,
                    CacheSeconds = 30,

                    StartTime = startDateTime,
                    EndTime = endDateTime
                }
            };
        }
    }

    public class CachedWidgetService : IWidgetService
    {
        public CachedWidgetService(
            IDistributedCache distributedCache,
            IWidgetService innerWidgetService)
        {
            DistributedCache = distributedCache;
            InnerWidgetService = innerWidgetService ?? throw new ArgumentNullException(nameof(innerWidgetService));
        }

        public IDistributedCache DistributedCache { get; }
        public IWidgetService InnerWidgetService { get; }

        public async Task<List<WidgetResponseItem>> Get(int id)
        {
            var cacheKey = $"{nameof(IWidgetService)}.Get.{id.ToString()}";

            var cachedResult = 
                await GetCachedResponseAsync(cacheKey);

            if (cachedResult == null)
            {
                var newResult = 
                    await 
                    InnerWidgetService.Get(id);

                await CacheResponseAsync(cacheKey, newResult, TimeSpan.FromSeconds(30));

                return newResult;
            }

            var result = JsonConvert.DeserializeObject<List<WidgetResponseItem>>(cachedResult);

            return result;
        }

        public async Task<string> GetCachedResponseAsync(string cacheKey)
        {
            var cachedResponse =
                await
                DistributedCache
                .GetStringAsync(cacheKey);

            return string.IsNullOrEmpty(cachedResponse) ? null : cachedResponse;
        }

        public async Task CacheResponseAsync(string cacheKey, object response, TimeSpan timeToLive)
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
                    AbsoluteExpirationRelativeToNow = timeToLive
                });
        }
    }

    public class WidgetResponseItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int SleepMilliseconds { get; set; }

        public int CacheSeconds { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
