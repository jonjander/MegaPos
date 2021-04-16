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
            ExekuteSync(posState => posState.OnProductAddedRemoved += ProductAdded);
            ExekuteSync(posState => posState.OnProductPriceChanged += RefreshPriceState);
            
        }

        //protected override async Task OnAfterRenderAsync(bool firstRender)
        //{
        //    //todo
        //    //await Animatescroll("board");
        //}

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
            ExekuteSync(posState => posState.OnProductAddedRemoved -= RefreshPriceState);
            ExekuteSync(posState => posState.OnProductPriceChanged -= RefreshPriceState);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        public async Task Animatescroll(string elementId)
            => await JSRuntime.InvokeVoidAsync("Leaderboard.animateScroll", elementId);

    }
}
