using MegaPOS.Extentions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
	public class Product : Entitet
	{
		public string Name { get; set; }
		public float OriginalPrice { get; set; }
		public int Quantity { get; set; }
		public Store Store { get; set; }
		public string StoreId { get; set; }
		public int ProductsSold { get; set; }
		public float LocalProfit { get; set; }
		public float MinPriceProcentage { get; set; } = 0.9f;
		public float MaxPriceProcentage { get; set; } = 5f;
		public float Price => GetPrice();
		private float GlobalProfit => Store?.ProfitTarget ?? 1.1f;

        public List<Order> Orders { get; set; }
        public string Color { get; set; }

        public void UpdateMinPrice(float procentage)
		{
			MinPriceProcentage = procentage;
		}

		public void UpdateMaxPrice(float procentage)
		{
			MaxPriceProcentage = procentage;
		}

		public Product()
        {
			LocalProfit = GlobalProfit * 1.01f;
			Color = "#000000";
		}

		public void SetProfit(float profit)
		{
			LocalProfit = profit;
		}

		private float GetPrice()
		{
			try
			{
				return Store.UpdateProductPrice(Id);
			} catch (Exception ex)
            {
				Console.WriteLine(ex.Message);
				return 999;
            }
		}

		internal void Decrease()
		{
			Quantity--;
			ProductsSold++;
		}

		internal void Increase()
		{
			Quantity++;
			ProductsSold--;
		}
	}
    public class ProductComparer : IEqualityComparer<Product>
    {
        public bool Equals(Product x, Product y)
        {
			return x.Id == y.Id;
        }

        public int GetHashCode([DisallowNull] Product obj)
        {
			return $"{obj.Id}{obj.Name}{obj.OriginalPrice}".GetHashCode();
        }
    }

}
