using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public static class Global
    {
        public static MemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
    }
}
