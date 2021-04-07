using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class ProductNameChanged : EventBase
    {
        public string Name { get; set; }
        public string ProductId { get; set; }
    }
}
