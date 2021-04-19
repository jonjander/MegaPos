using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
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
using MegaPOS.Shared.ModalComponents.StoreSetup;
using MegaPOS.Extentions;

namespace MegaPOS.Pages.Pos
{
    public class PosBase : PageBase
    {

        protected AddProductModal AddProductModal { get; set; }
        protected CustomersModal CustomersModal { get; set; }
        protected CheckoutModal CheckoutModal { get; set; }
        protected ChangeProductModal ChangeProductModal { get; set; }
        protected StoreSetup StoreSetup { get; set; }

        protected CustomerVm Customer { get; set; }

        protected int GlobalProfit { get; set; }

        protected override void SetupMessageHub()
        {
            base.SetupMessageHub();
            HubConnection.On<GlobalProfitChangeEvent>(SendMethods.GlobalProfitChanged.ToString(), (Event) =>
            {
                if (StoreId == Event.StoreId)
                {
                    GlobalProfit = Event.value;
                    StateHasChanged();
                }
            });
        }


        protected async Task ChangeGlobalProfit(int value)
        {
            GlobalProfit = value;
            var v = (float)value / 100;
            await ExecureAsync(posState => posState.ChangeGlobalProfit(v, HubConnection));
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            NewCustomer();
            var current = ExecuteSync(posState => posState.GetGlobalProfit());
            GlobalProfit = (int)(current * 100);
        }

        protected void NewCustomer()
        {
            Customer = ExecuteSync(posState => posState.GetNewCustomer());
        }

        protected async Task AddProduct(NewProductCommand product)
        {
            await ExecuteAsync(posState => posState.AddNewProduct(product, HubConnection));
                
        }

        protected void ChangeCustomer(string customerId)
        {
            Customer = ExecuteSync(posState => posState.LoadCustmer(customerId));
        }

        protected async Task ChangeCustomerName(string value)
        {
            Customer.Name = value;
            await ExecuteAsync(posState => posState.SaveCustomerName(Customer.Id, value));
        }

        protected async Task BuyProduct(ProductVm product)
        {
            await ExecuteAsync(posState => posState.BuyProduct(Customer.Id, product.Id, HubConnection));
            Customer.Orders = ExecuteSync(posState => posState.GetCustomerOrders(Customer.Id));
        }

        protected async Task Checkout()
        {
            if (Customer.Id != null)
                await CheckoutModal.ShowModal(Customer);
        }

        protected void CustomerPayed(string customerId)
        {
            ExecuteSync(posState => posState.Checkout(customerId));
            NewCustomer();
        }

        protected async Task ChangeProduct(ChangeProductCommand command)
        {
            if (command.LocalProfit != command.OriginalProduct.LocalProfit)
                ExecuteSync(posState => posState.ChangeProductLocalProfit(command.OriginalProduct.Id, command.LocalProfit));

            if (command.Name != command.OriginalProduct.Name)
                await ExecuteAsync(posState => posState.ChangeProductName(command.OriginalProduct.Id, command.Name, HubConnection));

            if (command.MinPriceProcentage != command.OriginalProduct.MinPriceProcentage)
                await ExecuteAsync(posState => posState.ChangeProductMinPriceProcentage(command.OriginalProduct.Id, command.MinPriceProcentage, HubConnection));
            
            if (command.MaxPriceProcentage != command.OriginalProduct.MaxPriceProcentage)
                await ExecuteAsync(posState => posState.ChangeProductMaxPriceProcentage(command.OriginalProduct.Id, command.MaxPriceProcentage, HubConnection));

            if (command.Quantity != command.OriginalProduct.Quantity)
                await ExecuteAsync(posState => posState.ChangeProductQuantity(command.OriginalProduct.Id,  command.Quantity - command.OriginalProduct.Quantity, HubConnection));

            if (command.Color != command.OriginalProduct.Color)
                await ExecuteAsync(posState => posState.ChangeProductColor(command.OriginalProduct.Id, command.Color, HubConnection));
        
                    }

        protected void ParkCustomer()
        {
            if (Customer.Orders != null && Customer.Orders.Any())
                Customer = ExecuteSync(posState => posState.GetNewCustomer());
        }


        protected async Task RemoveProduct(string orderId, string customerId)
        {
            await ExecuteAsync(posState => posState.RemoveProduct(orderId, customerId, HubConnection));
            Customer.Orders = Customer.Orders
                .Where(_ => _.Id != orderId)
                .ToList();
        }

        protected void OpenEditModal(ProductVm product)
        {
            ProductVm loadedproduct = ExecuteSync(_ => _.GetProduct(product.Id).ToVm());
            ChangeProductModal.ShowModal(loadedproduct);
        }

        protected void OpenSetupModal()
        {
            StoreSetup.OpenModal();
        }
    }
}
