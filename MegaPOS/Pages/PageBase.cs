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
    public class PageBase : ServiceCallerBase, IDisposable, IAsyncDisposable
    {
        
        protected HubConnection HubConnection { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        protected string StoreId => GetStoreId();

        protected StoreViewModel Model { get; set; } = new StoreViewModel();


        private string GetStoreId()
        {
            return ExecuteSync(_ => _.StoreId);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public virtual async ValueTask DisposeAsync()
        {
            if (HubConnection != null)
                await HubConnection.DisposeAsync();
        }

        protected override async Task OnInitializedAsync()
        {
            ExecuteSync(_ => _.Init(Id));
            HubConnection = MessageHub.SetupMessageHub(NavigationManager);
            SetupMessageHub();
            await HubConnection.StartAsync();
            LoadViewModel();
        }

        protected void LoadViewModel()
        {
            Model = new StoreViewModel
            {
                Products = ExecuteSync(posState => posState.GetAllProducts(posState.StoreId).ToVm()),
                LeaderboardRows = Model.AvalibleProducts.ToLeaderboardModel()
            };
        }


        protected virtual void SetupMessageHub()
        {
            HubConnection.On<QuantityEvent>(SendMethods.QuantityChanged.ToString(), async (Event) =>
            {
                if (StoreId == Event.StoreId)
                {
                    Model.Products.FirstOrDefault(_ => _.Id == Event.ProductId).Quantity = Event.NewQuantity;
                    if (Event.NewQuantity <= 0)
                        Model.LeaderboardRows.FirstOrDefault(_ => _.ProductId == Event.ProductId).IsDisabled = true;

                    ExecuteSync(_ => _.InvokeProductAddedRemoved());
                    ExecuteSync(_ => _.InvokeProductPriceChanged());

                    await ExecuteAsync(posState => posState.QuantityChanged(Event.ProductId, Event.NewQuantity));
                }
            });

            HubConnection.On<ProductNameChanged>(SendMethods.ProductNameChange.ToString(), async (Event) => {
                if (StoreId == Event.StoreId)
                {
                    Model.Products.FirstOrDefault(_ => _.Id == Event.ProductId).Name = Event.Name;
                    Model.LeaderboardRows.FirstOrDefault(_ => _.ProductId == Event.ProductId).Name = Event.Name;
                    await ExecuteAsync(posState => posState.NameChanged(Event.ProductId, Event.Name));
                }
            });

            HubConnection.On<ProductColorChangedEvent>(SendMethods.ProductColorChanged.ToString(), async (Event) => {
                if (StoreId == Event.StoreId)
                {
                    await ExecuteAsync(posState => posState.ColorChanged(Event.ProductId, Event.Color));
                }
            });

            HubConnection.On<ProductAddedEvent>(SendMethods.ProductAdded.ToString(), async (Event) =>
            {
                if (StoreId == Event.StoreId)
                {
                    if (Model.Products.DoNotContain(Event.Product))
                        Model.Products.Add(Event.Product);

                    var nyProduct = Event.Product.ToLeaderboardModel();
                    if (Model.LeaderboardRows.DoNotContain(nyProduct))
                    {
                        Model.LeaderboardRows.Add(nyProduct);
                        Model.LeaderboardRows = Model.LeaderboardRows.UpdateOrder();
                    }
                    
                    StateHasChanged();
                    ExecuteSync(posState => posState.InvokeProductAddedRemoved());
                    ExecuteSync(posState => posState.InvokeProductPriceChanged());

                    foreach (var item in Model.Products)
                    {
                        await ExecuteAsync(posState => posState.QuantityChanged(item.Id, item.Quantity));
                    }

                    foreach (var item in Model.Products)
                    {
                        await ExecuteAsync(posState => posState.NameChanged(item.Id, item.Name));
                    }
                }
            });

            HubConnection.On<PriceChangeEvent>(SendMethods.PriceChanged.ToString(), async (ChangeEventArgs) =>
            {
                if (StoreId == ChangeEventArgs.StoreId)
                {
                
                    Model.Products = Model.Products.UpdatePrice(ChangeEventArgs, ChangeEventArgs.NewPrice);
                    Model.LeaderboardRows = Model.LeaderboardRows.UpdatePrice(ChangeEventArgs, ChangeEventArgs.NewPrice);

                    ExecuteSync(posState => posState.InvokeProductPriceChanged());
                    
                    await ExecuteAsync(posState => posState.PriceChanged(ChangeEventArgs.ProductId, ChangeEventArgs.OldPrice, ChangeEventArgs.NewPrice));
                }
            });
        }

        
    }
}
