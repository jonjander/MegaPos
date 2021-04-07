using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class QuantityEvent : EventBase
    {
        public string ProductId { get; set; }
        public int NewQuantity { get; set; }
    }
}
