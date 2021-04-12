using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model
{
    public class StoreStats
    {
        public float TotalSold { get; set; }
        public float TotalAssetWorth { get; set; }
        public float TotalMargin { get; set; }
    }
    public class StoreSetupVm
    {
        public string StoreId { get; set; }
        public string Name { get; set; }
        public StoreStats StoreStats {get;set;}
        public string PayoutSwishNumber { get; set; }

        public StoreSetupVm()
        {
            StoreStats = new StoreStats();
        }
    }
}
