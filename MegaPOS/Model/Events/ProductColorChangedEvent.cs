using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class ProductColorChangedEvent : EventBase
    {
        public string Color { get; set; }
        public string ProductId { get; set; }
    }
}
