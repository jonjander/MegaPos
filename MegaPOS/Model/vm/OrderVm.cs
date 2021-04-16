using MegaPOS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.vm
{
    public class OrderVm : IIdentifiable
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public float Price { get; set; }
        public DateTime Created { get; set; }
       
    }
}
