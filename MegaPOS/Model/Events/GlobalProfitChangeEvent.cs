using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.Events
{
    public class GlobalProfitChangeEvent : EventBase
    {
        public int value { get; set; }
    }
}
