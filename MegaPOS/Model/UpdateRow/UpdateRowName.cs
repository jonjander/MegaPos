using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.UpdateRow
{
    public class UpdateRowName : IUpdateRow
    {
        public string ProductId { get; set; }

        public RowField RowField { get; set; }
        public string NewName { get; set; }
    }
}
