using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.UpdateRow
{
    public interface IUpdateRow
    {
        public string ProductId { get; }
        RowField RowField { get; }
    }
}
