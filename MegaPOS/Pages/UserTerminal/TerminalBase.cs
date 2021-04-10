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

namespace MegaPOS.Pages.UserTerminal
{
    public class TerminalBase : PageBase
    {
        [Inject] public IConfiguration config { get; set; }
        [Parameter] public string name { get; set; }
        private bool IsInUse { get; set; }
        protected bool DisplaySummary { get; set; }
        protected List<OrderVm> Orders { get; set; }
        protected string QRCodeStr { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            Orders = new List<OrderVm>();
        }

        public void GenereateQRCode(string customer)
        {
            var message = $"{name} {customer}".Truncate(50);
            var text = $"C{config.GetValue<string>("swish")};{Orders.Sum(_=>_.Price).ToString("0.00").Replace(",",".")};{message};0";
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
            hubConnection.On<FindTerminalEvent>(SendMethods.FindTerminal.ToString(), async (Event) =>
            {
                if (StoreId == Event.StoreId && !IsInUse && !string.IsNullOrEmpty(name))
                {
                    await hubConnection.SendAsync(nameof(MessageHub.SendTeminalFound), new TerminalFoundEvent { 
                        StoreId = StoreId,
                        ConnectionId = hubConnection.ConnectionId,
                        TerminalId = name,
                        CustomerId = Event.CustomerId
                    });
                }
            });

            hubConnection.On<OpenTermnialEvent>(SendMethods.OpenTerminal.ToString(), async (Event) => {
                if (StoreId == Event.StoreId && name == Event.TerminalId)
                {
                    if (!IsInUse)
                    {
                        IsInUse = true;
                        await hubConnection.SendAsync(nameof(MessageHub.SendOpenTerminalConfirmation), new OpenTerminalConfirmationEvent
                        {
                            StoreId = StoreId,
                            TerminalId = name,
                            CustomerId = Event.CustomerId
                        });
                    }
                }
            });
            hubConnection.On<CloseTerminalEvent>(SendMethods.CloseTerminal.ToString(), async (Event) => {
                if (StoreId == Event.StoreId && name == Event.TerminalId)
                {
                    IsInUse = false;
                    DisplaySummary = false;
                    Orders = new List<OrderVm>();
                    StateHasChanged();
                }
            });

            hubConnection.On<TerminalSummaryEvent>(SendMethods.TerminalSummary.ToString(), async (Event) => {
                if (StoreId == Event.StoreId && name == Event.TerminalId && IsInUse)
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
