using MegaPOS.DBContext;
using MegaPOS.Extentions;
using MegaPOS.Model.Events;
using MegaPOS.Model.vm;
using MegaPOS.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Pages
{
    public class PageBase : ServiceCallerBase, IDisposable
    {
        
        protected HubConnection hubConnection { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        //[Inject] public PosState posState { get; set; }
        protected string StoreId => GetStoreId();

        protected StoreViewModel Model { get; set; } = new StoreViewModel();


        private string GetStoreId()
        {
            return ExekveraSync(_ => _.StoreId);
        }

        public virtual void Dispose()
        {
            if (hubConnection != null)
                _ = hubConnection.DisposeAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            await Exekvera(_ => _.Init(Id));
            hubConnection = MessageHub.SetupMessageHub(NavigationManager);
            SetupMessageHub();
            await hubConnection.StartAsync();
            await LoadViewModel();
        }

        private async Task LoadViewModel()
        {
            Model = new StoreViewModel();
            Model.Products = await Exekvera(posState => posState.GetAllProducts(posState.StoreId));
            Model.LeaderboardRows = Model.AvalibleProducts.ToLeaderboardModel();
        }


        protected virtual void SetupMessageHub()
        {
            hubConnection.On<QuantityEvent>(SendMethods.QuantityChanged.ToString(), async (Event) =>
            {
                
                if (StoreId == Event.StoreId)
                {
                    Model.Products.FirstOrDefault(_ => _.ProductId == Event.ProductId).Quantity = Event.NewQuantity;
                    if (Event.NewQuantity <= 0)
                        Model.LeaderboardRows.FirstOrDefault(_ => _.ProductId == Event.ProductId).IsDisabled = true;
                    ExekveraSync(_ => _.InvokeProductAddedRemoved());
                    ExekveraSync(_ => _.InvokeProductPriceChanged());
                }
            });

            hubConnection.On<ProductNameChanged>(SendMethods.ProductNameChange.ToString(), async (Event) => {
                if (StoreId == Event.StoreId)
                {
                    Model.Products.FirstOrDefault(_ => _.ProductId == Event.ProductId).Name = Event.Name;
                    Model.LeaderboardRows.FirstOrDefault(_ => _.ProductId == Event.ProductId).Name = Event.Name;
                    await Exekvera(posState => posState.NameChanged(Event.ProductId, Event.Name));
                }
            });

            hubConnection.On<ProductAddedEvent>(SendMethods.ProductAdded.ToString(), async (Event) =>
            {
                if (StoreId == Event.StoreId)
                {
                    Model.Products.Add(Event.Product);
                    var nyProduct = Event.Product.ToLeaderboardModel();
                    Model.LeaderboardRows.Add(nyProduct);
                    Model.LeaderboardRows = Model.LeaderboardRows.UpdateOrder();
                    StateHasChanged();
                    ExekveraSync(posState => posState.InvokeProductAddedRemoved());
                    ExekveraSync(posState => posState.InvokeProductPriceChanged());
                    foreach (var item in Model.LeaderboardRows)
                    {
                        if (item.ProductId == Event.Product.ProductId)
                            await Exekvera(posState => posState.PriceChanged(item.ProductId, 0, item.Price));
                        else
                            await Exekvera(posState => posState.PriceChanged(item.ProductId, item.Price, item.Price));
                    }

                    foreach (var item in Model.Products)
                    {
                        await Exekvera(posState => posState.QuantityChanged(item.ProductId, item.Quantity));
                    }

                    foreach (var item in Model.Products)
                    {
                        await Exekvera(posState => posState.NameChanged(item.ProductId, item.Name));
                    }
                }
            });

            hubConnection.On<PriceChangeEvent>(SendMethods.PriceChanged.ToString(), async (ChangeEventArgs) =>
            {
                if (StoreId == ChangeEventArgs.StoreId)
                {
                    //update leaderboard
                    Model.Products = Model.Products.UpdatePrice(ChangeEventArgs);
                    Model.LeaderboardRows = Model.LeaderboardRows.UpdatePrice(ChangeEventArgs);
                    //Model.LeaderboardRows = Model.LeaderboardRows.UpdateOrder();

                    ExekveraSync(posState => posState.InvokeProductPriceChanged());
                    
                    await Exekvera(posState => posState.PriceChanged(ChangeEventArgs.ProductId, ChangeEventArgs.OldPrice, ChangeEventArgs.NewPrice));
                }
            });
        }
       
    }
}
