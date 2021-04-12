using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.UpdateRow
{
    public class UpdateRowQuantity : IUpdateRow
    {
        public string ProductId { get; set; }
        public RowField RowField { get; set; }
        public int Quantity { get; set; }
    }
}
