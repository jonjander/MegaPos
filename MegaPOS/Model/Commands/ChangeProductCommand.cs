using MegaPOS.Model.vm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Commands
{
    public class ChangeProductCommand
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public float MinPriceProcentage { get; set; }
        public float LocalProfit { get; set; }
        public string StoreId { get; set; }
        public ProductVm OriginalProduct { get; set; }

        public ChangeProductCommand(string StoreId)
        {
            this.StoreId = StoreId;
        }

        public void Set(ProductVm product)
        {
            OriginalProduct = product;
            Name = product.Name;
            Quantity = product.Quantity;
            LocalProfit = product.LocalProfit;
            MinPriceProcentage = product.MinPriceProcentage;
        }

        public bool IsValid()
        {
            return MinPriceProcentage > 0f &&
                MinPriceProcentage <= 1f &&
                Quantity >= 0f &&
                LocalProfit > 1f &&
                !string.IsNullOrEmpty(Name);
        }
    }
}
