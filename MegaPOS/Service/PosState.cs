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
using System.Threading;
using Microsoft.Extensions.Logging;
using MegaPOS.Model.Interfaces;

namespace MegaPOS.Service
{

    
    public class PosState
    {
        private readonly IUnitOfWork DatabaseContext;
        private readonly ILogger<PosState> logger;
        public List<Func<IUpdateRow, Task>> UpdateRow { get; set; } = new List<Func<IUpdateRow, Task>>();
        private Store Store { get; set; }
        public string StoreId => Store?.Id;
        public bool IsInitilized { get; set; }

        internal CustomerVm GetNewCustomer()
        {
            var emptyslot = DatabaseContext.Customers
                .Include(_ => _.Orders)
                .FirstOrDefault(_ => _.Orders.Count == 0 && string.IsNullOrEmpty(_.Name) && _.StoreId == StoreId);
            if (emptyslot != null)
            {
                return emptyslot.ToVm();
            }
            else
            {
                var newCustomer = DatabaseContext.Customers.Add(new Customer() { 
                    StoreId = StoreId
                });
                DatabaseContext.SaveChanges();
                return newCustomer.Entity.ToVm();
            }
        }

        private DbDebouncer<float> ChangeGlobalProfitDebouncer = new DbDebouncer<float>();
        
        internal async Task<float> ChangeGlobalProfit(float value, HubConnection hubConnection)
        {
            return await ChangeGlobalProfitDebouncer.PoolAndRun(async () => {
                //Get current state
                var storeProducts = DatabaseContext.Products
                    .Where(_ => _.StoreId == StoreId)
                    .ToList();
                var changeEvent = storeProducts.Select(_ =>
                    new PriceChangeEvent
                    {
                        ProductId = _.Id,
                        OldPrice = _.Price,
                        NewQuantity = _.Quantity
                    }).ToList();

                //change profit
                var store = DatabaseContext.Stores.FirstOrDefault(_ => _.Id == Store.Id);
                store.SetProfit(value);
                DatabaseContext.SaveChanges();

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

                await hubConnection.SendAsync(nameof(MessageHub.SendGlobalProfitChanged), new GlobalProfitChangeEvent
                {
                    StoreId = StoreId,
                    value = (int)(store.ProfitTarget * 100)
                });

                return store.ProfitTarget;
            });
        }

        internal CustomerVm LoadCustmer(string customerId)
        {
            var customer = DatabaseContext.Customers
                .FirstOrDefault(_ => _.Id == customerId);
            return customer.ToVm();
        }

      
        private async Task<string> SaveName(string id, string value)
        {
            try
            {
                var customer = DatabaseContext
                    .Customers
                    .FirstOrDefault(_ => _.Id == id);
                customer.Name = value;
                DatabaseContext.SaveChanges();
                logger.LogInformation($"Wrote {value}");
                return customer.Name;
            } catch (Exception ex)
            {
                logger.LogError(ex, $"{ex.Message}");
                return value;
            }
        }

        private DbDebouncer<string> SaveCustomerNameDebouncer = new DbDebouncer<string>();

        internal async Task SaveCustomerName(string id, string value)
        {
            Func<Task<string>> editTask = () => SaveName(id, value);
            await SaveCustomerNameDebouncer.PoolAndRun(editTask);

        }

        internal async Task ChangeProductName(string productId, string name, HubConnection hubConnection)
        {
            var product = DatabaseContext.Products.FirstOrDefault(_ => _.Id == productId);
            product.Name = name;
            DatabaseContext.SaveChanges();

            await hubConnection.SendAsync(nameof(MessageHub.SendProductNameChanged), new ProductNameChanged { 
                StoreId = StoreId,
                Name = name,
                ProductId = productId
            });
        }

        internal async Task ChangeProductMinPriceProcentage(string productId, float minPriceProcentage, HubConnection hubConnection)
        {
            var storeProducts = DatabaseContext.Products
                .Where(_ => _.StoreId == StoreId)
                .ToList();
            var changeEvent = storeProducts.Select(_ =>
                new PriceChangeEvent
                {
                    ProductId = _.Id,
                    OldPrice = _.Price,
                    NewQuantity = _.Quantity
                }).ToList();

            var product = DatabaseContext.Products.FirstOrDefault(_ => _.Id == productId);
            product.UpdateMinPrice(minPriceProcentage);
            DatabaseContext.SaveChanges();

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

        internal void ChangeProductQuantity(string productId, int diff)
        {
            //todo send change event for price and quantity

            var product = DatabaseContext.Products.FirstOrDefault(_ => _.Id == productId);
            product.Quantity += diff;
            if (diff <= 0)
            {
                var nToRemove = Math.Abs(diff);
                for (int i = 0; i < nToRemove; i++)
                {
                    var revertAsset = DatabaseContext.Orders
                        .FirstOrDefault(_ => 
                        _.ProductId == productId &&
                        _.Type == OrderType.Assets
                        );
                    DatabaseContext.Orders.Remove(revertAsset);

                    var revertExpences = DatabaseContext.Orders
                        .FirstOrDefault(_ =>
                        _.ProductId == productId &&
                        _.Type == OrderType.Expences
                        );
                    DatabaseContext.Orders.Remove(revertExpences);
                }
            }
            else
            {
                for (int i = 0; i < diff; i++)
                {
                    Store.AddProduct(product);
                }
            }
            DatabaseContext.SaveChanges();
        }

        internal async Task RemoveProduct(string orderId, string customerId, HubConnection hubConnection)
        {
            var storeProducts = DatabaseContext.Products
                .Where(_ => _.StoreId == StoreId)
                .ToList();
            var changeEvent = storeProducts.Select(_ =>
                new PriceChangeEvent
                {
                    ProductId = _.Id,
                    OldPrice = _.Price,
                    NewQuantity = _.Quantity
                }).ToList();

            var customer = DatabaseContext.Customers.FirstOrDefault(_ => _.Id == customerId && _.Closed != true);

            if (customer == null)
                return;

            customer.Orders = customer.Orders
                .Where(_ => _.Id != orderId)
                .ToList();

            var order = DatabaseContext.Orders.FirstOrDefault(_ => _.Id == orderId);
            if (order == null)
                return;

            order.Product.Increase();
            DatabaseContext.SaveChanges();

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

        internal void ChangeProductLocalProfit(string productId, float localProfit)
        {
            var product = DatabaseContext.Products.FirstOrDefault(_ => _.Id == productId);
            product.SetProfit(localProfit);
            DatabaseContext.SaveChanges();
        }

        internal float GetGlobalProfit()
        {
            var store = DatabaseContext.Stores.FirstOrDefault(_ => _.Id == StoreId);
            return store?.ProfitTarget ?? 1.1f;
        }

        public PosState(
            DatabaseContext databaseContext,
            ILogger<PosState> logger)
        {
            DatabaseContext = databaseContext;
            this.logger = logger;
        }

        public void Init(string name)
        {
            if (IsInitilized)
                return;
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
                existing = new Store(1.1f, name);
                DatabaseContext.AddStore(existing);
                DatabaseContext.SaveChanges();
            }
            Store = existing;
            IsInitilized = true;
        }

        internal void Checkout(string id)
        {
            var customer = DatabaseContext.Customers.FirstOrDefault(_ => _.Id == id);
            customer.Closed = true;
            DatabaseContext.SaveChanges();
        }

        public List<PriceChangeEvent> SellProduct(Customer c, Product p)
        {
            var storeProducts = DatabaseContext.Products
                .Where(_ => _.StoreId == StoreId)
                .ToList();
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

            foreach (var item in storeProducts)
            {
                var change = changeEvent.FirstOrDefault(_ => _.ProductId == item.Id);
                change.NewPrice = item.Price;
                change.NewQuantity = item.Quantity;
            }

            return changeEvent;
        }

        internal async Task BuyProduct(string costumerId, string productId, HubConnection hubConnection)
        {
            var product = DatabaseContext.Products.FirstOrDefault(_ => _.Id == productId);
            var customer = DatabaseContext.Customers.FirstOrDefault(_ => _.Id == costumerId);
            var priceChangeEvents = SellProduct(customer, product);
            DatabaseContext.SaveChanges();

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

        internal List<OrderVm> GetCustomerOrders(string custumerId)
        {
            var orders = DatabaseContext
                .Orders
                .Where(_ => _.CustomerId == custumerId)
                .ToList();
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
      
            var item = DatabaseContext.AddProduct(p);
            Store.AddProduct(p);

            DatabaseContext.SaveChanges();
            var addedEvent = new ProductAddedEvent(item.Entity.ToVm());
            addedEvent.StoreId = StoreId;
            await hubConnection.SendAsync(nameof(MessageHub.SendProductAdded), addedEvent);
        }

        internal List<ProductVm> GetAllProducts(string storeId)
        {
            var result = DatabaseContext.Products
                         .Where(_ => _.StoreId == storeId)
                         .ToList();
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

        public async Task NameChanged(string productId, string newName)
        {
            var command = new UpdateRowName
            {
                ProductId = productId,
                NewName = newName
            };

            await Task.WhenAll(UpdateRow.Select(_ => _.Invoke(command)));
        }


        private bool isLoadingCustomers;
        public List<CustomerVm> LoadOpenCustomers()
        {
            if (isLoadingCustomers)
                return new List<CustomerVm>();
            isLoadingCustomers = true;
            var customers = DatabaseContext.Customers
                .Include(_=>_.Orders)
                .Where(_ => _.Closed != true && _.StoreId == StoreId)
                .ToList();
            isLoadingCustomers = false;
            return customers.ToVm();
        }

        public ProductVm GetProduct (string id)
            => DatabaseContext.Products.FirstOrDefault(_ => _.Id == id).ToVm();
        

    }
}
