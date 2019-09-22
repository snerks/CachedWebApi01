using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CachedWebApi01.Cache
{
    public class RedisCacheSettings
    {
        public bool IsEnabled { get; set; }

        public string ConnectionString { get; set; }
    }
}
