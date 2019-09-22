using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CachedWebApi01.Cache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CachedWebApi01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagedItemsController : ControllerBase
    {
        // GET api/values/5
        [Cached(10)]
        [HttpGet("{id}")]
        public async Task<ActionResult<PagedItemsResponse<WidgetResponseItem>>> Get(int id)
        {
            var result = new PagedItemsResponse<WidgetResponseItem>
            {
                PageNumber = 1,
                PageSize = 10
            };

            result.Items.Add(new WidgetResponseItem { Id = id, Name = DateTime.Now.ToString() });

            return 
                result;
        }
    }

    public class PagedItemsResponse<T>
    {
        public int PageSize { get; set; }
        public int PageNumber { get; set; }

        public List<T> Items { get; set; } = new List<T>();
    }

    public class WidgetResponseItem
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}