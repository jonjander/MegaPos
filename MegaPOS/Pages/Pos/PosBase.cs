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

namespace MegaPOS.Pages.Pos
{
    public class PosBase : PageBase
    {

        protected AddProductModal addProductModal { get; set; }
        protected CustomersModal customersModal { get; set; }
        protected CheckoutModal checkoutModal { get; set; }
        protected ChangeProductModal changeProductModal { get; set; }

        protected CustomerVm Customer { get; set; }

        protected int GlobalProfit { get; set; }

        protected override void SetupMessageHub()
        {
            base.SetupMessageHub();
            hubConnection.On<GlobalProfitChangeEvent>(SendMethods.GlobalProfitChanged.ToString(), async (Event) =>
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
            await Exekvera(posState => posState.ChangeGlobalProfit(v, hubConnection));
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await NewCustomer();
            var current = await Exekvera(posState => posState.GetGlobalProfit());
            GlobalProfit = (int)(current * 100);
        }

        protected async Task NewCustomer()
        {
            Customer = await Exekvera(posState => posState.GetNewCustomer());
        }

        protected async Task AddProduct(NewProductCommand product)
        {
            await Exekvera(posState => posState.AddNewProduct(product, hubConnection));
                
        }

        protected async Task ChangeCustomer(string customerId)
        {
            Customer = await Exekvera(posState => posState.LoadCustmer(customerId));
        }

        protected async Task ChangeCustomerName(string value)
        {
            Customer.Name = value;
            await Exekvera(posState => posState.SaveCustomerName(Customer.Id, value));
        }

        protected async Task BuyProduct(ProductVm product)
        {
            await Exekvera(posState => posState.BuyProduct(Customer.Id, product.ProductId, hubConnection));
            Customer.Orders = await Exekvera(posState => posState.GetCustomerOrders(Customer.Id));
        }

        protected async Task Checkout()
        {
            if (Customer.Id != null)
                await checkoutModal.ShowModal(Customer);
        }

        protected async Task CustomerPayed(string customerId)
        {
            await Exekvera(posState => posState.Checkout(customerId));
            await NewCustomer();
        }

        protected async Task ChangeProduct(ChangeProductCommand command)
        {
            if (command.LocalProfit != command.OriginalProduct.LocalProfit)
                await Exekvera(posState => posState.ChangeProductLocalProfit(command.OriginalProduct.ProductId, command.LocalProfit));

            if (command.Name != command.OriginalProduct.Name)
                await Exekvera(posState => posState.ChangeProductName(command.OriginalProduct.ProductId, command.Name, hubConnection));

            if (command.MinPriceProcentage != command.OriginalProduct.MinPriceProcentage)
                await Exekvera(posState => posState.ChangeProductMinPriceProcentage(command.OriginalProduct.ProductId, command.MinPriceProcentage, hubConnection));

            if (command.Quantity != command.OriginalProduct.Quantity)
                await Exekvera(posState => posState.ChangeProductQuantity(command.OriginalProduct.ProductId,  command.Quantity - command.OriginalProduct.Quantity));
        }

        protected async Task ParkCustomer()
        {
            if (Customer.Orders != null && Customer.Orders.Any())
                Customer = await Exekvera(posState => posState.GetNewCustomer());
        }


        protected async Task RemoveProduct(string orderId, string customerId)
        {
            await Exekvera(posState => posState.RemoveProduct(orderId, customerId, hubConnection));
            Customer.Orders = Customer.Orders
                .Where(_ => _.Id != orderId)
                .ToList();
        }

        protected void OpenEditModal(ProductVm product)
        {
            ProductVm loadedproduct = ExekveraSync(_ => _.GetProduct(product.ProductId));
            changeProductModal.ShowModal(loadedproduct);
        }
    }
}
