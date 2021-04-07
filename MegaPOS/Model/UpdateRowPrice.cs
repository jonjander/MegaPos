using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public class UpdateRowPrice : IUpdateRow
    {
        public RowField RowField => RowField.price;
        public float OldPrice { get; set; }
        public float NewPrice { get; set; }

        public string ProductId { get; set; }
    }
}
