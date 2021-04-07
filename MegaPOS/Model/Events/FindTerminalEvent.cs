using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class FindTerminalEvent : EventBase
    {
        public string CustomerId { get; set; }
    }
}
