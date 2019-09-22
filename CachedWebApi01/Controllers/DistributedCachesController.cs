using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CachedWebApi01.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CachedWebApi01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistributedCachesController : ControllerBase
    {
        public DistributedCachesController(IDistributedCache distributedCache)
        {
            DistributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        public IDistributedCache DistributedCache { get; }

        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<WidgetResponseItem>> Get([FromRoute] int id)
        {
            //var results = DistributedCache.
            //return new string[]
            //{
            //    //"value1",
            //    DateTime.Now.ToString()
            //};

            //var redisCache = DistributedCache as Microsoft.Extensions.Caching.StackExchangeRedis.RedisCache;

            //if (redisCache != null)
            //{
            //    var redisCacheServer = redisCache as IServer;

            //    if (redisCacheServer != null)
            //    {
            //        var keys = redisCacheServer.Keys();
            //    }
            //}

            var cacheKey = $"{nameof(IWidgetService)}.Get.{id.ToString()}";

            var cachedResponse = await GetCachedResponseAsync(cacheKey);

            if (cachedResponse == null)
            {
                return NotFound();
            }

            var resultFormatted = JToken.Parse(cachedResponse).ToString(Formatting.Indented);

            var result = JsonConvert.DeserializeObject<List<WidgetResponseItem>>(resultFormatted);

            return result.FirstOrDefault();

            //return new List<string>
            //{
            //    resultFormatted
            //};
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