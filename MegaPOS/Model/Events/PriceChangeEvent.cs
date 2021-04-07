using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class PriceChangeEvent : EventBase
    {
        public string ProductId { get; set; }
        public float OldPrice { get; set; }
        public float NewPrice { get; set; }
        public int NewQuantity { get; set; }
    }
}
