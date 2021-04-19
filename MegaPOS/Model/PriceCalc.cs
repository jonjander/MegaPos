using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public class PriceCalc
    {
        public string Id { get; internal set; }
        public float MinPriceProcentage { get; internal set; }
        public float OriginalPrice { get; internal set; }
        public float Price { get; internal set; }
        public float Quantity { get; internal set; }
        public float MaxPriceProcentage { get; internal set; }
    }
}
