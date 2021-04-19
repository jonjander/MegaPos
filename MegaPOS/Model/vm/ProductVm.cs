using MegaPOS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.vm
{
    public class ProductVm : IIdentifiable
    {
        public string Id { get; set; }
        public float Price { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public float LocalProfit { get; set; }
        public bool IsDisabled => Quantity <= 0;

        public float MinPriceProcentage { get; set; }
        public float MaxPriceProcentage { get; set; }
        public string Color { get; set; }
        
    }
}
