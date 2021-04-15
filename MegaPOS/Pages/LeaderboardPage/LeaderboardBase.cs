using MegaPOS.Extentions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Pages.LeaderboardPage
{
    public class LeaderboardBase : PageBase, IDisposable
    {
        [Parameter] public string TerminalName { get; set; }
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            ExekveraSync(posState => posState.OnProductAddedRemoved += ProductAdded);
            ExekveraSync(posState => posState.OnProductPriceChanged += RefreshPriceState);
            
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            //await Animatescroll("board");
        }

        public void ReloadBoard()
        {
            LoadViewModel();
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
            ExekveraSync(posState => posState.OnProductAddedRemoved -= RefreshPriceState);
            ExekveraSync(posState => posState.OnProductPriceChanged -= RefreshPriceState);
            base.Dispose();
        }

        public async Task Animatescroll(string elementId)
            => await JSRuntime.InvokeVoidAsync("Leaderboard.animateScroll", elementId);

    }
}
