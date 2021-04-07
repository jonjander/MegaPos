using MegaPOS.Enum;
using MegaPOS.Model.vm;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazorise;
using MegaPOS.Service;
using Microsoft.AspNetCore.SignalR.Client;
using MegaPOS.Model.Events;

namespace MegaPOS.Shared.ModalComponents.CheckoutModalComponent
{
    public class CheckoutModalBase : ComponentBase, IDisposable
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public PosState posState { get; set; }
        [Parameter] public EventCallback<string> OnCustomerPayed {get;set;}
        protected string SelectedCustomerId { get; set; }
        protected Blazorise.Modal modalRef;
        protected CheckoutStages stage { get; set; } 
        protected string TerminalId;
        private HubConnection hubConnection;

        protected List<string> AvalibleTerminals = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            stage = CheckoutStages.Idle;
            hubConnection = MessageHub.SetupMessageHub(NavigationManager);
            ConfigureMessageHub();
            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection != null)
                _ = hubConnection.DisposeAsync();
        }

        public async Task ShowModal(CustomerVm customerVm)
        {
            await CoseTerminal();
            stage = CheckoutStages.Idle;
            AvalibleTerminals = new List<string>();
            TerminalId = null;
            SelectedCustomerId = customerVm.Id;
            
            modalRef.Show();


            StartScanner();
        }

        private async void StartScanner()
        {
            while (stage != CheckoutStages.TerminalSelected)
            {
                if (hubConnection.State == HubConnectionState.Connected)
                {
                    await RequestTerminal();
                }
                await Task.Delay(2000);
            }
        }

        public void ConfigureMessageHub()
        {
            hubConnection.On<OpenTerminalConfirmationEvent>(SendMethods.TerminalOpenConfirmation.ToString(), async (Event) => {
                if (posState.StoreId == Event.StoreId)
                {
                    if (SelectedCustomerId == Event.CustomerId)
                    {
                        //open terminal
                        stage = CheckoutStages.TerminalSelected;
                        TerminalId = Event.TerminalId;
                        var orders = await posState.GetCustomerOrders(SelectedCustomerId);

                        await hubConnection.SendAsync(nameof(MessageHub.SendTerminalSummary), new TerminalSummaryEvent { 
                            StoreId = posState.StoreId,
                            CustomerId = SelectedCustomerId,
                            Orders = orders,
                            TerminalId = TerminalId
                        });

                        StateHasChanged();
                    } else
                    {
                        //terminal take by another customer
                        AvalibleTerminals = AvalibleTerminals.Where(_ => _ != Event.TerminalId).ToList();
                        StateHasChanged();
                    }
                }
            });

            hubConnection.On<TerminalFoundEvent>(SendMethods.TerminalFound.ToString(), async (Event) => {
                if (posState.StoreId == Event.StoreId &&
                    SelectedCustomerId == Event.CustomerId &&
                    (stage == CheckoutStages.TerminalFound || stage == CheckoutStages.TerminalScanning)
                )
                {
                    if (!AvalibleTerminals.Any(_ => _ == Event.TerminalId))
                    {
                        AvalibleTerminals.Add(Event.TerminalId);
                        stage = CheckoutStages.TerminalFound;
                        StateHasChanged();
                    }
                }
            });
        }

        protected async Task SelectTerminal(string terminalId)
        {
            await hubConnection.SendAsync(nameof(MessageHub.SendOpenTerminal), new OpenTermnialEvent
            {
                StoreId = posState.StoreId,
                CustomerId = SelectedCustomerId,
                TerminalId = terminalId
            });
        }

        protected async Task CoseTerminal()
        {
            if (!string.IsNullOrEmpty(TerminalId))
            {
                await hubConnection.SendAsync(nameof(MessageHub.SendCloseTerminal), new CloseTerminalEvent
                {
                    StoreId = posState.StoreId,
                    TerminalId = TerminalId
                });
            }
        }
        
        private async Task RequestTerminal()
        {
            if ((stage == CheckoutStages.TerminalScanning || stage == CheckoutStages.TerminalFound || stage == CheckoutStages.Idle) && !string.IsNullOrEmpty(SelectedCustomerId))
            {
                if (stage == CheckoutStages.Idle)
                    stage = CheckoutStages.TerminalScanning;
                await hubConnection.SendAsync(nameof(MessageHub.SendFindTerminal), new FindTerminalEvent
                {
                    StoreId = posState.StoreId,
                    CustomerId = SelectedCustomerId
                });
            }
        } 

        public async Task MarkAsPaid()
        {
            await CoseTerminal();
            if (!string.IsNullOrEmpty(SelectedCustomerId))
                await OnCustomerPayed.InvokeAsync(SelectedCustomerId);
            HideModal();
        }


        public async Task CloseModal()
        {
            await CoseTerminal();
            HideModal();
        }

        public void HideModal()
        {
            stage = CheckoutStages.Idle;
            modalRef.Hide();
        }
    }
}
