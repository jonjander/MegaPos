using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
	public class Receipt
	{
		public string CustomerId { get; set; }
		public List<ReceiptRows> Rows { get; set; }
		public float Total => Rows?.Sum(o => o.Price) ?? 0;

		public Receipt(Customer cust)
		{
			CustomerId = cust.Id;
			Rows = cust.Orders.Select(o => new ReceiptRows
			{
				ProductName = o.Product.Name,
				Price = o.Credit
			}).ToList();
		}
	}
}
