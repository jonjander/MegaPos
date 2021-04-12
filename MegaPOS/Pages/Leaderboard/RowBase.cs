using MegaPOS.Model.UpdateRow;
using MegaPOS.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Pages.Leaderboard
{
    public class RowBase : ComponentBase, IDisposable
    {
        [Parameter] public LeaderboardRowModel RowModel { get; set; }
        [Inject] public PosState posState { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        protected int AnimeringsHastighet { get; set; } = 2500;

        public void Dispose()
        {
            posState.UpdateRow.Remove(UpdateRowHandler);
        }

        protected override void OnInitialized()
        {
            posState.UpdateRow.Add(UpdateRowHandler);
        }

        protected async Task AnimerPris(float tidigarePris, float nyttPris)
        {
            await AnimateNumber(RowModel.ProductId, tidigarePris, nyttPris, AnimeringsHastighet);
        }

        public async Task UpdateRowHandler(IUpdateRow updated)
        {
            if (updated.ProductId == RowModel.ProductId)
            {
                if (updated is UpdateRowPrice)
                {
                    var updatedprice = updated as UpdateRowPrice;
                    RowModel.Update(updatedprice);
                    await AnimerPris(updatedprice.OldPrice, updatedprice.NewPrice);
                }

                if (updated is UpdateRowName)
                {
                    var updatedprice = updated as UpdateRowName;
                    RowModel.Name = updatedprice.NewName;
                }
                
            }
        }

        public async Task AnimateNumber(string objectId, int start, int end, int durationMs)
            => await JSRuntime.InvokeVoidAsync("Leaderboard.animateValue", objectId, start, end, durationMs);

        public async Task AnimateNumber(string objectId, float start, float end, int durationMs)
            => await JSRuntime.InvokeVoidAsync("Leaderboard.animateValue", objectId, start, end, durationMs);


    }
}
