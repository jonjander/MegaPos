using MegaPOS.Model.vm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class ProductAddedEvent : EventBase
    {
        public ProductVm Product { get; set; }

        public ProductAddedEvent(ProductVm Product)
        {
            this.Product = Product;
        }
    }
}
