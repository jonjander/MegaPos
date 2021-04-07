using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class OpenTermnialEvent : EventBase
    {
        public string CustomerId { get; set; }
        public string TerminalId { get; set; }
    }
}
