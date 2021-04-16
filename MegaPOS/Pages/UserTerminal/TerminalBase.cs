using MegaPOS.Model.Events;
using MegaPOS.Model.vm;
using MegaPOS.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QRCoder;
using System.Drawing.Imaging;
using System.IO;
using System.Drawing;
using Microsoft.Extensions.Configuration;
using MegaPOS.Extentions;
using MegaPOS.Pages.LeaderboardPage;

namespace MegaPOS.Pages.UserTerminal
{
    public class TerminalBase : PageBase
    {
        [Parameter] public string Name { get; set; }
        private bool IsInUse { get; set; }
        protected bool DisplaySummary { get; set; }
        protected List<OrderVm> Orders { get; set; }
        protected string QRCodeStr { get; set; }
        protected Leaderboard Leaderboard { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Orders = new List<OrderVm>();
        }

        public void GenereateQRCode(string customer)
        {
            var message = $"{Name} {customer}".Truncate(50);
            var storeSetup = ExecuteSync(_ => _.GetStoreSetup(StoreId));

            var text = $"C{storeSetup.PayoutSwishNumber};{Orders.Sum(_=>_.Price).ToString("0.00").Replace(",",".")};{message};0";
            if (!string.IsNullOrEmpty(text))
            {
                using var ms = new MemoryStream();
                using var bitmap = QRCodeHelper.GetQRCode(text, 20, Color.Black, Color.White, QRCodeGenerator.ECCLevel.H);
                bitmap.Save(ms, ImageFormat.Png);
                QRCodeStr = $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
            }
            StateHasChanged();
        }

        protected override void SetupMessageHub()
        {
            base.SetupMessageHub();
            HubConnection.On<FindTerminalEvent>(SendMethods.FindTerminal.ToString(), async (Event) =>
            {
                if (StoreId == Event.StoreId && !IsInUse && !string.IsNullOrEmpty(Name))
                {
                    await HubConnection.SendAsync(nameof(MessageHub.SendTeminalFound), new TerminalFoundEvent { 
                        StoreId = StoreId,
                        ConnectionId = HubConnection.ConnectionId,
                        TerminalId = Name,
                        CustomerId = Event.CustomerId
                    });
                }
            });

            HubConnection.On<OpenTermnialEvent>(SendMethods.OpenTerminal.ToString(), async (Event) => {
                if (StoreId == Event.StoreId && Name == Event.TerminalId)
                {
                    if (!IsInUse)
                    {
                        IsInUse = true;
                        await HubConnection.SendAsync(nameof(MessageHub.SendOpenTerminalConfirmation), new OpenTerminalConfirmationEvent
                        {
                            StoreId = StoreId,
                            TerminalId = Name,
                            CustomerId = Event.CustomerId
                        });
                    }
                }
            });
            HubConnection.On<CloseTerminalEvent>(SendMethods.CloseTerminal.ToString(), (Event) => {
                if (StoreId == Event.StoreId && Name == Event.TerminalId)
                {
                    IsInUse = false;
                    DisplaySummary = false;
                    Orders = new List<OrderVm>();
                    Leaderboard.ReloadBoard();
                    StateHasChanged();
                }
            });

            HubConnection.On<TerminalSummaryEvent>(SendMethods.TerminalSummary.ToString(), (Event) => {
                if (StoreId == Event.StoreId && Name == Event.TerminalId && IsInUse)
                {
                    Orders = Event.Orders;
                    GenereateQRCode(Event.CustomerId);
                    DisplaySummary = true;
                    StateHasChanged();
                }
            });

        }

    }
}
