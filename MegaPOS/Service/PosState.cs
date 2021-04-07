using MegaPOS.DBContext;
using MegaPOS.Model;
using MegaPOS.Pages.Leaderboard;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using MegaPOS.Extentions;
using MegaPOS.Model.vm;
using MegaPOS.Model.Commands;
using MegaPOS.Model.Events;
using MegaPOS.Enum;

namespace MegaPOS.Service
{

    
    public class PosState
    {
        private readonly DatabaseContext DatabaseContext;

        public List<Func<IUpdateRow, Task>> UpdateRow { get; set; } = new List<Func<IUpdateRow, Task>>();
        private Store Store { get; set; }
        public string StoreId => Store?.Id;


        internal async Task<CustomerVm> GetNewCustomer()
        {
            var emptyslot = await DatabaseContext.Customers
                .Include(_ => _.Orders)
                .FirstOrDefaultAsync(_ => _.Orders.Count == 0 && string.IsNullOrEmpty(_.Name) && _.StoreId == StoreId);
            if (emptyslot != null)
            {
                return emptyslot.ToVm();
            }
            else
            {
                var newCustomer = await DatabaseContext.Customers.AddAsync(new Customer() { 
                    StoreId = StoreId
                });
                await DatabaseContext.SaveChangesAsync();
                return newCustomer.Entity.ToVm();
            }
        }

        internal async Task<float> ChangeGlobalProfit(float value, HubConnection hubConnection)
        {
            //Get current state
            var storeProducts = await DatabaseContext.Set<Product>()
                .Where(_ => _.StoreId == StoreId)
                .ToListAsync();
            var changeEvent = storeProducts.Select(_ =>
                new PriceChangeEvent
                {
                    ProductId = _.Id,
                    OldPrice = _.Price,
                    NewQuantity = _.Quantity
                }).ToList();

            //change profit
            var store = await DatabaseContext.Set<Store>().FirstOrDefaultAsync(_ => _.Id == Store.Id);
            store.SetProfit(value);
            await DatabaseContext.SaveChangesAsync();

            //Update prices
            Store.Updatediscount();
            await DatabaseContext.SaveChangesAsync();

            //Report
            foreach (var item in storeProducts)
            {
                var change = changeEvent.FirstOrDefault(_ => _.ProductId == item.Id);
                change.NewPrice = item.Price;
                change.NewQuantity = item.Quantity;
            }

            foreach (var item in changeEvent)
            {
                item.StoreId = StoreId;
                await hubConnection.SendAsync(nameof(MessageHub.SendPriceChanged), item);
            }

            return store.ProfitTarget;
        }

        internal async Task<CustomerVm> LoadCustmer(string customerId)
        {
            var customer = await DatabaseContext.Set<Customer>()
                .FirstOrDefaultAsync(_ => _.Id == customerId);
            return customer.ToVm();
        }

        private bool savingCustomerName { get; set; }
        private string lastSavedName { get; set; }
        internal async Task<string> SaveCustomerName(string id, string value)
        {
            if (savingCustomerName)
                return lastSavedName;
            savingCustomerName = true;
            var customer = await DatabaseContext
                .Set<Customer>()
                .FirstOrDefaultAsync(_ => _.Id == id);
            customer.Name = value;
            await DatabaseContext.SaveChangesAsync();
            lastSavedName = value;
            savingCustomerName = false;
            return customer.Name;
        }

        internal async Task ChangeProductName(string productId, string name)
        {
            var product = await DatabaseContext.Set<Product>().FirstOrDefaultAsync(_ => _.Id == productId);
            product.Name = name;
            await DatabaseContext.SaveChangesAsync();
        }

        internal async Task ChangeProductMinPriceProcentage(string productId, float minPriceProcentage)
        {
            var product = await DatabaseContext.Set<Product>().FirstOrDefaultAsync(_ => _.Id == productId);
            product.UpdateMinPrice(minPriceProcentage);
            await DatabaseContext.SaveChangesAsync();
        }

        internal async Task ChangeProductQuantity(string productId, int diff)
        {
            var product = await DatabaseContext.Set<Product>().FirstOrDefaultAsync(_ => _.Id == productId);
            product.Quantity += diff;
            if (diff <= 0)
            {
                var nToRemove = Math.Abs(diff);
                for (int i = 0; i < nToRemove; i++)
                {
                    var revertAsset = await DatabaseContext.Set<Order>()
                        .FirstOrDefaultAsync(_ => 
                        _.ProductId == productId &&
                        _.Type == OrderType.Assets
                        );
                    DatabaseContext.Set<Order>().Remove(revertAsset);

                    var revertExpences = await DatabaseContext.Set<Order>()
                        .FirstOrDefaultAsync(_ =>
                        _.ProductId == productId &&
                        _.Type == OrderType.Expences
                        );
                    DatabaseContext.Set<Order>().Remove(revertExpences);
                }
            }
            else
            {
                for (int i = 0; i < diff; i++)
                {
                    Store.AddProduct(product);
                }
            }
            await DatabaseContext.SaveChangesAsync();
        }

        internal async Task RemoveProduct(string orderId, string customerId, HubConnection hubConnection)
        {
            var storeProducts = await DatabaseContext.Set<Product>()
                .Where(_ => _.StoreId == StoreId)
                .ToListAsync();
            var changeEvent = storeProducts.Select(_ =>
                new PriceChangeEvent
                {
                    ProductId = _.Id,
                    OldPrice = _.Price,
                    NewQuantity = _.Quantity
                }).ToList();

            var customer = await DatabaseContext.Set<Customer>()
                .FirstOrDefaultAsync(_ => _.Id == customerId && _.Closed != true);

            if (customer == null)
                return;

            customer.Orders = customer.Orders
                .Where(_ => _.Id != orderId)
                .ToList();

            var order = await DatabaseContext.Set<Order>()
                .FirstOrDefaultAsync(_ => _.Id == orderId);

            order.Product.Increase();
            await DatabaseContext.SaveChangesAsync();
            Store.Updatediscount();
            await DatabaseContext.SaveChangesAsync();

            foreach (var item in storeProducts)
            {
                var change = changeEvent.FirstOrDefault(_ => _.ProductId == item.Id);
                change.NewPrice = item.Price;
                change.NewQuantity = item.Quantity;
            }

            foreach (var item in changeEvent)
            {
                item.StoreId = StoreId;
                await hubConnection.SendAsync(nameof(MessageHub.SendPriceChanged), item);
            }

        }

        internal async Task ChangeProductLocalProfit(string productId, float localProfit)
        {
            var product = await DatabaseContext.Set<Product>().FirstOrDefaultAsync(_ => _.Id == productId);
            product.SetProfit(localProfit);
            await DatabaseContext.SaveChangesAsync();
        }

        internal async Task<float> GetGlobalProfit()
        {
            var store = await DatabaseContext.Set<Store>().FirstOrDefaultAsync(_ => _.Id == Store.Id);
            return store.ProfitTarget;
        }

        public PosState(
            DatabaseContext databaseContext)
        {
            DatabaseContext = databaseContext;
        }

        public async Task Init(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new Exception("name error");
            var existing = DatabaseContext.Stores
                .Include(_ => _.Products)
                .Include(_ => _.Orders)
                .ThenInclude(_ => _.Product)
                .OrderBy(_ => _.Id)
                .FirstOrDefault(_=>_.Name == name);
            if (existing == null)
            {
                existing = new Store(1.0f, name);
                DatabaseContext.Add(existing);
                await DatabaseContext.SaveChangesAsync();
            }
            Store = existing;
        }

        internal async Task Checkout(string id)
        {
            var customer = await DatabaseContext.Customers.FirstOrDefaultAsync(_ => _.Id == id);
            customer.Closed = true;
            await DatabaseContext.SaveChangesAsync();
            //todo new Receipt(cust)
        }

        public async Task<List<PriceChangeEvent>> SellProduct(Customer c, Product p)
        {
            var storeProducts = await DatabaseContext.Set<Product>()
                .Where(_ => _.StoreId == StoreId)
                .ToListAsync();
            var changeEvent = storeProducts.Select(_ =>
                new PriceChangeEvent
                {
                    ProductId = _.Id,
                    OldPrice = _.Price,
                    NewQuantity = _.Quantity
                }).ToList();

            if (p == null)
                throw new Exception("product cannot be null");
            var product = storeProducts
                .Where(_=>_.Quantity > 0)
                .FirstOrDefault(pr => pr.Id == p.Id);

            if (product == null)
                return new List<PriceChangeEvent> { };

            product.Decrease();

            var order = new Order(p, OrderType.Revenues, product.Price);
            c.LäggTillOrer(order);

            Store.Orders.Add(order);
            Store.Updatediscount();

            foreach (var item in storeProducts)
            {
                var change = changeEvent.FirstOrDefault(_ => _.ProductId == item.Id);
                change.NewPrice = item.Price;
                change.NewQuantity = item.Quantity;
            }
            //changeEvent = changeEvent
            //    .Where(_ =>
            //    _.NewPrice != _.OldPrice ||
            //    _.ProductId == p.Id)
            //    .ToList();

            return changeEvent;
        }

        internal async Task BuyProduct(string costumerId, string productId, HubConnection hubConnection)
        {
            var product = await DatabaseContext.Set<Product>().FirstOrDefaultAsync(_ => _.Id == productId);
            var customer = await DatabaseContext.Set<Customer>().FirstOrDefaultAsync(_ => _.Id == costumerId);
            var priceChangeEvents = await SellProduct(customer, product);
            await DatabaseContext.SaveChangesAsync();

            foreach (var item in priceChangeEvents)
            {
                item.StoreId = StoreId;
                await hubConnection.SendAsync(nameof(MessageHub.SendPriceChanged), item);

                if (item.ProductId == productId)
                {
                    await hubConnection.SendAsync(nameof(MessageHub.SendQuantityChanged),
                        new QuantityEvent
                        {
                            StoreId = item.StoreId,
                            ProductId = item.ProductId,
                            NewQuantity = item.NewQuantity
                        });
                }
            }
        }

        internal async Task<List<OrderVm>> GetCustomerOrders(string custumerId)
        {
            var orders = await DatabaseContext
                .Set<Order>()
                .Where(_ => _.CustomerId == custumerId)
                .ToListAsync();
            return orders.ToVm();
        }

        internal async Task AddNewProduct(NewProductCommand product, HubConnection hubConnection)
        {
            var p = new Product
            {
                LocalProfit = product.LocalProfit,
                OriginalPrice = product.Price,
                Quantity = product.Quantity,
                Name = product.Name,
                StoreId = StoreId
            };
            var item = await DatabaseContext.AddAsync(p);
            Store.AddProduct(p);

            await DatabaseContext.SaveChangesAsync();
            var addedEvent = new ProductAddedEvent(item.Entity.ToVm());
            addedEvent.StoreId = StoreId;
            await hubConnection.SendAsync(nameof(MessageHub.SendProductAdded), addedEvent);
        }

        internal async Task<List<ProductVm>> GetAllProducts(string storeId)
        {
            var result = await DatabaseContext.Set<Product>()
                         .Where(_ => _.StoreId == storeId)
                         .ToListAsync();
            return result.ToVm();
        }
        

        public event EventHandler OnProductAddedRemoved;
        public void InvokeProductAddedRemoved()
            => OnProductAddedRemoved?.Invoke(this, null);

        public event EventHandler OnProductPriceChanged;
        public void InvokeProductPriceChanged()
            => OnProductPriceChanged?.Invoke(this, null);

        public async Task PriceChanged(string productId, float oldPrice, float newPrice)
        {
            var command = new UpdateRowPrice
            {
                ProductId = productId,
                OldPrice = oldPrice,
                NewPrice = newPrice
            };

            await Task.WhenAll(UpdateRow.Select(_ => _.Invoke(command)));
        }

        public async Task QuantityChanged(string productId, int newQuantity)
        {
            var command = new UpdateRowQuantity
            {
                ProductId = productId,
                Quantity = newQuantity
            };

            await Task.WhenAll(UpdateRow.Select(_ => _.Invoke(command)));
        }


        private bool isLoadingCustomers;
        public async Task<List<CustomerVm>> LoadOpenCustomers()
        {
            if (isLoadingCustomers)
                return new List<CustomerVm>();
            isLoadingCustomers = true;
            var customers = await DatabaseContext.Set<Customer>()
                .Include(_=>_.Orders)
                .Where(_ => _.Closed != true && _.StoreId == StoreId)
                .ToListAsync();
            isLoadingCustomers = false;
            return customers.ToVm();
        }

    }
}
