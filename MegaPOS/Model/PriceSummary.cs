using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
	public class PriceSummary
	{
		public DateTime DateTime { get; set; }
		public float Price { get; set; }

		public PriceSummary(Order o)
		{
			DateTime = o.Date;
			Price = o.Credit;
		}

	}
}
