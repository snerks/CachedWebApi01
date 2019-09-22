using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CachedWebApi01.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace CachedWebApi01.Cache
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        public const int OneSecond = 1;
        public const int OneMinute = 60 * OneSecond;
        public const int OneHour = 60 * OneMinute;
        public const int OneDay = 24 * OneHour;

        private readonly int _timeToLiveSeconds;

        public CachedAttribute(int timeToLiveSeconds)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var isResponseCacheEnabled = false;

            try
            {
                var responseCacheSettings = 
                    context
                    .HttpContext
                    .RequestServices
                    .GetRequiredService<RedisCacheSettings>();

                isResponseCacheEnabled = responseCacheSettings.IsEnabled;
            }
            catch (Exception ex)
            {
                // Log
                //throw;
            }

            if (!isResponseCacheEnabled)
            {
                await next();
                return;
            }

            IResponseCacheService responseCacheService = null;

            try
            {
                responseCacheService = 
                    context
                    .HttpContext
                    .RequestServices
                    .GetRequiredService<IResponseCacheService>();
            }
            catch (Exception ex)
            {
                // Log
                //throw;
            }

            var responseCacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

            var willUseCache = 
                responseCacheService != null && 
                responseCacheKey != null;

            try
            {
                if (willUseCache)
                {
                    var cachedResponse = 
                        await 
                        responseCacheService
                        .GetCachedResponseAsync(responseCacheKey);

                    if (!string.IsNullOrEmpty(cachedResponse))
                    {
                        var contentResult = new ContentResult
                        {
                            Content = cachedResponse,
                            ContentType = "application/json",
                            StatusCode = 200
                        };

                        context.Result = contentResult;

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log
                //throw;
            }

            var executedContext = await next();

            if (willUseCache)
            {
                //if (executedContext.Result is OkObjectResult okObjectResult)
                if (executedContext.Result is ObjectResult okObjectResult)
                {
                    await 
                        responseCacheService
                        .CacheResponseAsync(
                            responseCacheKey, 
                            okObjectResult.Value, 
                            TimeSpan.FromSeconds(_timeToLiveSeconds));
                }
            }
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var keyBuilder = new StringBuilder();

            keyBuilder.Append($"{request.Path.ToString().ToLower()}");

            foreach (var (key, value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"|{key.ToLower()}-{value.ToString().ToLower()}");
            }

            return keyBuilder.ToString();
        }
    }
}
