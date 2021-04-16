using MegaPOS.Model;
using MegaPOS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Extentions
{
    public static class EntityExtentions
    {
        public static bool DoNotContain<T>(this IEnumerable<T> list, T obj) where T : IIdentifiable
            => !list.Any(_ => _.Id == obj.Id);
        public static bool DoNotContain<T>(this IEnumerable<T> list, string id) where T : IIdentifiable
            => !list.Any(_ => _.Id == id);
    }
}
