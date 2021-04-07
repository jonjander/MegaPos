using MegaPOS.Model.vm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class TerminalSummaryEvent : EventBase
    {
        public List<OrderVm> Orders { get; set; }
        public string CustomerId { get; set; }
        public string TerminalId { get; set; }
    }
}
