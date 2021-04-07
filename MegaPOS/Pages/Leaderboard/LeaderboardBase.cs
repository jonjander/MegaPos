using MegaPOS.Extentions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Pages.Leaderboard
{
    public class LeaderboardBase : PageBase, IDisposable
    {

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            posState.OnProductAddedRemoved += ProductAdded;
            posState.OnProductPriceChanged += RefreshPriceState;
        }

        private void ProductAdded(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        private void RefreshPriceState(object sender, EventArgs e)
        {
            var updateOrdernumber = Model.LeaderboardRows
                .OrderBy(_ => _.IsDisabled)
                .ThenBy(_ => _.Price)
                .ToList();
            for (int i = 0; i < updateOrdernumber.Count; i++)
            {
                updateOrdernumber[i].Order = i;
            }
            StateHasChanged();
        }


        public override void Dispose()
        {
            posState.OnProductAddedRemoved -= RefreshPriceState;
            posState.OnProductPriceChanged -= RefreshPriceState;
            base.Dispose();
        }

    }
}
