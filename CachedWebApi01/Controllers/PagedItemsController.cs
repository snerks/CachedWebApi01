using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CachedWebApi01.Cache;
using CachedWebApi01.Services;
using Microsoft.AspNetCore.Mvc;

namespace CachedWebApi01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagedItemsController : ControllerBase
    {
        public PagedItemsController(IWidgetService widgetService)
        {
            WidgetService = widgetService ?? throw new ArgumentNullException(nameof(widgetService));
        }

        public IWidgetService WidgetService { get; }

        // GET api/values/5
        [Cached(10 * CachedAttribute.OneSecond)]
        //[Cached(CachedAttribute.OneDay)]
        [HttpGet("{id}")]
        public async Task<ActionResult<PagedItemsResponse<WidgetResponseItem>>> Get(int id)
        {
            // Emulate expensive work
            var startDateTime = DateTime.Now;

            var sleepMilliseconds = 2000;
            Thread.Sleep(sleepMilliseconds);

            var items = await WidgetService.Get(id);
            var endDateTime = DateTime.Now;

            var result = new PagedItemsResponse<WidgetResponseItem>
            {
                PageNumber = 1,
                PageSize = sleepMilliseconds,

                StartDateTime = startDateTime,
                EndDateTime = endDateTime
            };

            result.Items.AddRange(items);

            return result;
        }
    }

    public class PagedItemsResponse<T>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
        public List<T> Items { get; set; } = new List<T>();

        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }
}