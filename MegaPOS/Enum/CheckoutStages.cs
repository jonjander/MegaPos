using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Enum
{
    public enum CheckoutStages
    {
        Idle,
        TerminalScanning,
        TerminalFound,
        TerminalSelected,
        Compleated
    }
}
