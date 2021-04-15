using MegaPOS.Model.UpdateRow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Pages.LeaderboardPage
{
    public class LeaderboardRowModel
    {
        public int Order { get; set; }
        public float Price { get; set; }
        public string ProductId { get; set; }
        public string Name { get; set; }
        public bool IsDisabled { get; set; }

        public LeaderboardRowModel()
        {
            ProductId = Guid.NewGuid().ToString();
        }

        internal void Update(UpdateRowPrice updated)
        {
            Price = updated.NewPrice;
        }
    }
}
