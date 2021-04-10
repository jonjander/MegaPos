using MegaPOS.Extentions;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
		public float Discount { get; set; } = 0;
		public float MinPriceProcentage { get; set; } = 0.9f;

		public float Price => GetPrice();

		public float _Price { get; set; }

		private float Weight => ProductsSold / (float)Store.TotalProductsSold;
		private float ClampedWeight => Weight.Map(0, 0.5f, 1, -1);
		private float GlobalProfit => Store?.ProfitTarget ?? 1.1f;

        public List<Order> Orders { get; set; }

        public void UpdateMinPrice(float procentage)
		{
			MinPriceProcentage = procentage;
		}

		public void UpdateProfit(float newProfit)
		{
			LocalProfit = newProfit;
		}

		public void UpdateDiscount(float ProfitMarginShare)
		{
			Discount = ProfitMarginShare * Quantity * ClampedWeight;
		}

        public Product()
        {
			LocalProfit = GlobalProfit * 1.01f;
		}

		public Product(string name, Store store, float price, int quantity)
		{
			Id = Guid.NewGuid().ToString();
			Name = name;
			Store = store;
			Quantity = quantity;
			OriginalPrice = price;
			LocalProfit = GlobalProfit * 1.01f;
		}

		public void SetQuantity(string value)
		{
			OriginalPrice = float.Parse(value);
		}
		public void SetBasePrice(string value)
        {
			if (float.TryParse(value, out var price))
            {
				OriginalPrice = price;
			}
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
				//Store.UpdateProductPrice();
				//return _Price;

				//var VolumePrice = 1f;
				//if (Quantity < 2)
				//	VolumePrice = LocalProfit * 1.2f;
				//var profit = (LocalProfit + GlobalProfit) / 2f;
				//var newPrice = OriginalPrice * profit * VolumePrice - Discount;

				//var minPrice = OriginalPrice * MinPriceProcentage;
				//if (newPrice < minPrice)
				//	return MathF.Round(minPrice, 2);
				//else
				//	return MathF.Round(newPrice, 2);
			} catch
            {
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
}
