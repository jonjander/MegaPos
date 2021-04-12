using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.UpdateRow
{
    public class UpdateRowColor : IUpdateRow
    {
        public string ProductId { get; set; }
        public string Color { get; set; }
        public RowField RowField { get; set; }
    }
}
