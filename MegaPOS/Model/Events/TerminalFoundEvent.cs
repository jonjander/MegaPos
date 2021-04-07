using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class TerminalFoundEvent : EventBase
    {
        public string ConnectionId { get; set; }
        public string TerminalId { get; set; }
        public string CustomerId { get; set; }
    }
}
