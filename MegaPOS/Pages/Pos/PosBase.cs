using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using MegaPOS.Pages.Leaderboard;
using MegaPOS.Model;
using MegaPOS.Service;
using MegaPOS.Shared.ModalComponents;
using MegaPOS.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR.Client;
using MegaPOS.Model.Commands;
using MegaPOS.Model.vm;
using MegaPOS.Model.Events;
using MegaPOS.Shared.ModalComponents.CheckoutModalComponent;
using MegaPOS.Shared.ModalComponents;

namespace MegaPOS.Pages.Pos
{
    public class PosBase : PageBase
    {

        public List<Func<IUpdateRow, Task>> UpdateRow => posState.UpdateRow;
        protected AddProductModal addProductModal { get; set; }
        protected CustomersModal customersModal { get; set; }
        protected CheckoutModal checkoutModal { get; set; }
        protected ChangeProductModal changeProductModal { get; set; }

        protected CustomerVm Customer { get; set; }

        protected int GlobalProfit { get; set; }
        private bool IsChangingProfit { get; set; }

        protected override void SetupMessageHub()
        {
            base.SetupMessageHub();
            hubConnection.On<GlobalProfitChangeEvent>(SendMethods.GlobalProfitChanged.ToString(), async (Event) =>
            {
                if (posState.StoreId == Event.StoreId)
                {
                    GlobalProfit = Event.value;
                    StateHasChanged();
                }
            });
        }


        protected async Task ChangeGlobalProfit(int value)
        {
            if (IsChangingProfit || value == 0)
                return;
            IsChangingProfit = true;
            var v = (float)value / 100;
            var current = await posState.ChangeGlobalProfit(v, hubConnection);
            GlobalProfit = (int)(current * 100);
            IsChangingProfit = false;

            await hubConnection.SendAsync(nameof(MessageHub.SendGlobalProfitChanged), new GlobalProfitChangeEvent { 
                StoreId = posState.StoreId,
                value = GlobalProfit
            });
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await NewCustomer();
            var current = await posState.GetGlobalProfit();
            GlobalProfit = (int)(current * 100);
            IsChangingProfit = false;
        }

        protected async Task NewCustomer()
        {
            Customer = await posState.GetNewCustomer();
        }

        protected async Task AddProduct(NewProductCommand product)
        {
            await posState.AddNewProduct(product, hubConnection);
                
        }

        protected async Task ChangeCustomer(string customerId)
        {
            Customer = await posState.LoadCustmer(customerId);
        }

        protected async Task ChangeCustomerName(string value)
        {
            Customer.Name = await posState.SaveCustomerName(Customer.Id, value);
        }

        protected async Task BuyProduct(ProductVm product)
        {
            await posState.BuyProduct(Customer.Id, product.ProductId, hubConnection);
            Customer.Orders = await posState.GetCustomerOrders(Customer.Id);
        }

        protected async Task Checkout()
        {
            if (Customer.Id != null)
                await checkoutModal.ShowModal(Customer);
        }

        protected async Task CustomerPayed(string customerId)
        {
            await posState.Checkout(customerId);
            await NewCustomer();
        }

        protected async Task ChangeProduct(ChangeProductCommand command)
        {
            if (command.LocalProfit != command.OriginalProduct.LocalProfit)
                await posState.ChangeProductLocalProfit(command.OriginalProduct.ProductId, command.LocalProfit);

            if (command.Name != command.OriginalProduct.Name)
                await posState.ChangeProductName(command.OriginalProduct.ProductId, command.Name);

            if (command.MinPriceProcentage != command.OriginalProduct.MinPriceProcentage)
                await posState.ChangeProductMinPriceProcentage(command.OriginalProduct.ProductId, command.MinPriceProcentage);

            if (command.Quantity != command.OriginalProduct.Quantity)
                await posState.ChangeProductQuantity(command.OriginalProduct.ProductId,  command.Quantity - command.OriginalProduct.Quantity);
        }

        protected async Task ParkCustomer()
        {
            if (Customer.Orders != null && Customer.Orders.Any())
                Customer = await posState.GetNewCustomer();
        }
    }
}
