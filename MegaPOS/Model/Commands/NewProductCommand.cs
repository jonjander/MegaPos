using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Commands
{
    public class NewProductCommand
    {
        public string Name { get;set;}
        public int Quantity { get; set; }
        public float Price { get; set; }
        public float LocalProfit { get; set; } = 1.10f;
        public string StoreId { get; set; }

        public NewProductCommand(string StoreId)
        {
            this.StoreId = StoreId;
        }

        public bool IsValid()
        {
            return Price > 0f &&
                Quantity > 0f &&
                LocalProfit > 0 &&
                LocalProfit < 10 &&
                !string.IsNullOrEmpty(Name);
        }
    }
}
