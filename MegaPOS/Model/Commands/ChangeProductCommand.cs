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
        public float _MinPriceProcentage { get; set; }
        public float MinPriceProcentage  {
            get{ 
                return (float)_MinPriceProcentage / 100; 
            }    
            set{
                _MinPriceProcentage = value * 100f;
            }
        }
        public float _MaxPriceProcentage { get; set; }
        public float MaxPriceProcentage
        {
            get
            {
                return (float)_MaxPriceProcentage / 100;
            }
            set
            {
                _MaxPriceProcentage = value * 100f;
            }
        }
        public float _LocalProfit { get; set; }
        public float LocalProfit
        {
            get
            {
                return (float)_LocalProfit / 100;
            }
            set
            {
                _LocalProfit = value * 100f;
            }
        }
        public string StoreId { get; set; }
        public string Color { get; set; }
        public string Image { get; set;}
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
            MinPriceProcentage = (float)product.MinPriceProcentage;
            Color = product.Color;
        }

        public bool IsValid()
        {
            return MinPriceProcentage > 0f &&
                MinPriceProcentage <= 10f &&
                Quantity >= 0f &&
                LocalProfit > 0f &&
                LocalProfit <= 10f &&
                !string.IsNullOrEmpty(Name);
        }
    }
}
