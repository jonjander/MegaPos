using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaPOS.Model.Events;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;

namespace MegaPOS.Service
{
    public enum SendMethods
    {
        PriceChanged,
        ProductAdded,
        QuantityChanged,
        GlobalProfitChanged,
        FindTerminal,
        TerminalFound,
        OpenTerminal,
        TerminalOpenConfirmation,
        CloseTerminal,
        TerminalSummary
    }



    public class MessageHub : Hub
    {
        public async Task SendProductAdded(ProductAddedEvent productAddedEvent)
        {
            await Clients.All.SendAsync(SendMethods.ProductAdded.ToString(), productAddedEvent);
        }
        public async Task SendPriceChanged(PriceChangeEvent priceChangeEvent)
        {
            await Clients.All.SendAsync(SendMethods.PriceChanged.ToString(), priceChangeEvent);
        }

        public async Task SendQuantityChanged(QuantityEvent notForSaleEvent)
        {
            await Clients.All.SendAsync(SendMethods.QuantityChanged.ToString(), notForSaleEvent);
        }

        public async Task SendGlobalProfitChanged(GlobalProfitChangeEvent globalProfitChangeEvent)
        {
            await Clients.All.SendAsync(SendMethods.GlobalProfitChanged.ToString(), globalProfitChangeEvent);
        }

        public async Task SendFindTerminal(FindTerminalEvent Event)
        {
            await Clients.All.SendAsync(SendMethods.FindTerminal.ToString(), Event);
        }

        public async Task SendTeminalFound(TerminalFoundEvent Event)
        {
            await Clients.All.SendAsync(SendMethods.TerminalFound.ToString(), Event);
        }

        public async Task SendOpenTerminal(OpenTermnialEvent Event)
        {
            await Clients.All.SendAsync(SendMethods.OpenTerminal.ToString(), Event);
        }

        public async Task SendOpenTerminalConfirmation(OpenTermnialEvent Event)
        {
            await Clients.All.SendAsync(SendMethods.TerminalOpenConfirmation.ToString(), Event);
        }

        public async Task SendCloseTerminal(CloseTerminalEvent Event)
        {
            await Clients.All.SendAsync(SendMethods.CloseTerminal.ToString(), Event);
        }

        public async Task SendTerminalSummary(TerminalSummaryEvent Event)
        {
            await Clients.All.SendAsync(SendMethods.TerminalSummary.ToString(), Event);
        }

        public static HubConnection SetupMessageHub(NavigationManager NavigationManager)
        {
            var hubConnection = new HubConnectionBuilder()
                .WithUrl(NavigationManager.ToAbsoluteUri("/messageHub"))
                .Build();
            return hubConnection;
        }
    }
}
