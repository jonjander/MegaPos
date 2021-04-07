using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.vm
{
    public class CustomerVm
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<OrderVm> Orders { get; set; }

        public CustomerVm()
        {
            Orders = new List<OrderVm>();
        }
    }
}
