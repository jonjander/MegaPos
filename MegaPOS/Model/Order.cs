using MegaPOS.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
	public class Order : Entitet
	{
		public DateTime Date { get; set; }
		public float Debit { get; set; }
		public float Credit { get; set; }
		public OrderType Type { get; set; }
		public Product Product { get; set; }
		public string ProductId { get; set; }
        public Customer Customer { get;  set; }
        public string CustomerId { get;  set; }
        public Store Store { get; set; }
        public string StoreId { get; set; }
        public DateTime Created { get; set; }
		public int Quantity { get; set; }


		public Order()
        {

        }
		public Order(string storeId, Product product, OrderType type, float value, int quantity = 1)
		{
			Date = DateTime.Now;
			Product = product;
			Type = type;
			Quantity = quantity; 
			if (type == OrderType.Assets)
			{
				Debit = value * quantity;
			}
			else if (type == OrderType.Revenues || type == OrderType.Expences)
			{
				Credit = value * quantity;
			}
		}
	}
}
