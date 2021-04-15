using MegaPOS.Pages.LeaderboardPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Model.vm
{
    public class StoreViewModel
    {
        public List<ProductVm> Products { get; set; }
        public List<LeaderboardRowModel> LeaderboardRows { get; set; }

        public List<ProductVm> AvalibleProducts
            => Products?.Where(p => p.Quantity > 0)
                .OrderByDescending(_ => _.Price)
                .ToList() ?? new List<ProductVm>();


        public StoreViewModel()
        {
            Products = new List<ProductVm>();
            LeaderboardRows = new List<LeaderboardRowModel>();
        }
    }
}
