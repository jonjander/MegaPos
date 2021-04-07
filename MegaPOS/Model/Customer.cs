using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
	public class Customer : Entitet
	{
		public string Name { get; set; }
		public List<Order> Orders { get; set; } = new List<Order>();
		public bool? Closed { get; set; } = null;
        public string StoreId { get; set; }
        public Store Store { get; set; }

        internal void LäggTillOrer(Order order)
		{
			Orders.Add(order);
		}
	}
}
