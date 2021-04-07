using MegaPOS.Enum;
using MegaPOS.Model.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public class Store : Entitet
	{
		public List<Order> Orders { get; set; }
		public List<Product> Products { get; set; }


		public float TotalAssets => Orders?
			.Where(_ => _.Type == OrderType.Assets)
			.Sum(o => o.Debit) ?? 0;
		public float Balance => Orders?.Sum(o => o.Credit - o.Debit) ?? 0;
		public float TotalAsset => Products?.Sum(p => p.OriginalPrice * p.Quantity) ?? 0;
		public float PlannedProfit => Products?.Sum(p => p.OriginalPrice * p.ProductsSold * ProfitTarget) ?? 0;
		public float ProfitMargin => Balance - PlannedProfit;
		public float ProfitMarginAndTip => ProfitMargin + (Tip * TipPayback);
		private float TipPayback = 0.5f;
		private float Tip { get; set; }
		public float AssetProfitMarginShare
			=> ProfitMarginAndTip / Products?.Sum(ap => ap.Quantity) ?? 1;
		public float SoldProfitMarginShare
			=> ProfitMarginAndTip / Products?.Sum(ap => ap.ProductsSold) ?? 1;

		public void SetProductProfit(string name, float profit)
		{
			Products
				.Where(p => p.Name == name)
				.FirstOrDefault()
				.SetProfit(profit);
		}

		public void AddTip(float value)
		{
			Tip += value;
		}

		public int TotalProductsSold => Products.Sum(p => p.ProductsSold);
		public float ProfitTarget { get; private set; }
		
		[NotMapped]
		public List<Product> AvalibleProducts
			=> Products?.Where(p => p.Quantity > 0)
				.OrderByDescending(_ => _.Price)
				.ToList() ?? new List<Product>();

        public string Name { get; set; }

        public void SetProfit(float profit)
		{
			this.ProfitTarget = profit;
			
		}

		public void AddProduct(Product product)
		{
			Products.Add(product);
			Orders.Add(new Order(product, OrderType.Assets, product.OriginalPrice, product.Quantity));
			Orders.Add(new Order(product, OrderType.Expences, product.OriginalPrice, product.Quantity));
		}

		public Store(float globalprofit, string name)
		{
			Name = name;
			Orders = new List<Order>();
			Products = new List<Product>();
			ProfitTarget = globalprofit;
		}

        public Store()
        {

        }
		

		public void Updatediscount()
		{
			var marginshare = AssetProfitMarginShare;
			var outher = AvalibleProducts.ToList();
			if (marginshare > 0)
			{
				foreach (var element in outher)
				{
					element.UpdateDiscount(marginshare);
				}
			}
		}

		

		

		internal List<PriceSummary> GetProductPriceSummary(Product product)
		{
			return Orders.Where(p => p.Product.Id == product.Id)
			.Where(p => p.Type == OrderType.Revenues)
			.OrderBy(p => p.Date)
			.Select(p => new PriceSummary(p))
			.ToList();
		}
	}
}
