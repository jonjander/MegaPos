using MegaPOS.DBContext;
using MegaPOS.Enum;
using MegaPOS.Model;
using MegaPOS.Model.Events;
using MegaPOS.Model.vm;
using MegaPOS.Pages.Leaderboard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.Extentions
{
    public static class StoreExtentions
    {
        public static LeaderboardRowModel ToLeaderboardModel(this ProductVm _)
        => new LeaderboardRowModel
            {
                Order = 0,
                Price = _.Price,
                Name = _.Name,
                ProductId = _.ProductId
            };
        public static List<LeaderboardRowModel> ToLeaderboardModel(this IEnumerable<ProductVm> products)
        {
            var rows = products.Select(_ => _.ToLeaderboardModel());
            var updateOrdernumber = rows.UpdateOrder();
            return updateOrdernumber;
        }

        public static List<LeaderboardRowModel> UpdateOrder(this IEnumerable<LeaderboardRowModel> rows)
        {
            var updateOrdernumber = rows.OrderBy(_=>_.IsDisabled)
                .ThenBy(_ => _.Price)
                .ToList();
            for (int i = 0; i < updateOrdernumber.Count; i++)
            {
                updateOrdernumber[i].Order = i;
            }
            return updateOrdernumber;
        }

        public static List<LeaderboardRowModel> UpdatePrice(this List<LeaderboardRowModel> rows, PriceChangeEvent updateEvent)
        {
            var rowToUpdate = rows.FirstOrDefault(_ => _.ProductId == updateEvent.ProductId);
            if (rowToUpdate != null)
            {
                rowToUpdate.Price = updateEvent.NewPrice;
            }
            return rows;
        }

        public static List<ProductVm> UpdatePrice(this List<ProductVm> rows, PriceChangeEvent updateEvent)
        {
            var rowToUpdate = rows.FirstOrDefault(_ => _.ProductId == updateEvent.ProductId);
            if (rowToUpdate != null)
            {
                rowToUpdate.Price = updateEvent.NewPrice;
                rowToUpdate.Quantity = updateEvent.NewQuantity;
            }
            return rows;
        }

        public static ProductVm ToVm(this Product _)
            => new()
            {
                Price = _.Price,
                Name = _.Name,
                ProductId = _.Id,
                Quantity = _.Quantity,
                LocalProfit = _.LocalProfit,
                MinPriceProcentage = _.MinPriceProcentage
            };
        public static List<ProductVm> ToVm(this IEnumerable<Product> products)
            => products.Select(_ => _.ToVm()).ToList();

        public static CustomerVm ToVm(this Customer customer)
            => new()
            {
                Id = customer.Id,
                Name = customer.Name,
                Orders = customer.Orders.ToVm()
            };
        public static List<CustomerVm> ToVm(this IEnumerable<Customer> customers)
            => customers.Select(_ => _.ToVm()).ToList();

        public static OrderVm ToVm(this Order order)
            => order == null ? default : new OrderVm { 
                Price = order?.Credit ?? 0f,
                ProductName = order?.Product?.Name,
                Id = order.Id
            };

        public static List<OrderVm> ToVm(this IEnumerable<Order> orders)
            => orders
            .Select(_ => _.ToVm())
            .ToList();

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        public static QueueWrapper<T> Wrap<T>(this T obj)
            => new QueueWrapper<T> { Object = obj };

        public static void DropFist<T>(this QueueWrapper<List<T>> wrapper)
        {
            var topValue = wrapper.Object.FirstOrDefault();
            wrapper.Object = wrapper.Object.Where(l => !l.Equals(topValue)).ToList();
        }

        public static T PopFist<T>(this QueueWrapper<List<T>> wrapper)
        {
            var topValue = wrapper.Object.FirstOrDefault();
            return topValue;
        }


        public static float UpdateProductPrice(this Store store, string productId)
        {
            //var store = db.Set<Store>()
            //    .AsSplitQuery()
            //    .Include(_ => _.Products)
            //    .ThenInclude(_ => _.Orders)
            //    .FirstOrDefault(_ => _.Id == storeId);

            float globalProfit = store.ProfitTarget;
            float totalProductsInStore = store.Orders.Where(_ => _.Type == OrderType.Assets).Count();
            float antalUnikaProdukter = store.Products.Count;

            //var products = db.Set<Product>()
            //    .AsSplitQuery()
            //    .Include(_ => _.Orders)
            //    .Where(_ => _.StoreId == storeId);

            var products = store.Products;

            var stage1 = products
                .Select(_ => new
                {
                    _.Id,
                    _.OriginalPrice,
                    _.Quantity,
                    _.LocalProfit,
                    _.Orders,
                    TotalNumber = _.Quantity + _.ProductsSold,
                    NumberSold = _.ProductsSold,
                }).Select(_ => new
                {
                    _.Id,
                    _.OriginalPrice,
                    _.Quantity,
                    _.LocalProfit,
                    _.TotalNumber,
                    _.NumberSold,
                    _.Orders,
                    PercentageSold = _.NumberSold / _.TotalNumber,
                    Popularity = _.NumberSold / totalProductsInStore
                });

            var PercentageSoldTotal = stage1.Sum(_ => _.PercentageSold) + 0.0001f;
            var PopularityTotal = stage1.Sum(_ => _.Popularity) + 0.0001f;

            var stage2 = stage1.Select(_ => new
            {
                _.Id,
                _.OriginalPrice,
                _.Quantity,
                _.LocalProfit,
                _.TotalNumber,
                _.NumberSold,
                _.Orders,
                h = _.PercentageSold / PercentageSoldTotal,
                hshift = (_.PercentageSold / PercentageSoldTotal) - (1f / antalUnikaProdukter),
                w = _.Popularity / PopularityTotal,
                wshift = (_.Popularity / PopularityTotal) - (1f / antalUnikaProdukter)
            }).Select(_ => new {
                _.Id,
                _.OriginalPrice,
                _.Quantity,
                _.LocalProfit,
                npw = 1f + _.wshift,
                nph = 1f + _.hshift,
                npwh = (2f + _.wshift + _.hshift) / 2f,
                hwshift = (_.hshift + _.wshift) / 2f,
                PlannedProfit = _.OriginalPrice * _.TotalNumber * _.LocalProfit,
                PlannedProfitNow = _.OriginalPrice * _.NumberSold * _.LocalProfit,
                Gains = _.Orders.Where(_ => _.Type == OrderType.Revenues).Sum(_ => _.Credit)
            }).Select(_ => new {
                _.Id,
                _.Quantity,
                _.npwh,
                _.hwshift,
                _.LocalProfit,
                Margin = _.Gains - _.PlannedProfitNow,
                Left = _.PlannedProfit - _.Gains,
                LeftPerProduct = (_.PlannedProfit - _.Gains) / _.Quantity
            }).ToList();

            var totalMargin = stage2.Sum(_ => _.Margin);

            var stage5 = stage2.Select(_ => new
            {
                _.Id,
                NewPriceNoDiscount = _.LeftPerProduct * _.npwh * _.LocalProfit,
                Discount = totalMargin * -_.hwshift
            }).Select(_ => new
            {
                _.Id,
                Price = _.NewPriceNoDiscount - _.Discount
            }).ToList();

            var updateProducts = products.ToList();

            return stage5.FirstOrDefault(_ => _.Id == productId)?.Price ?? 0;

            //foreach (var item in updateProducts)
            //{
            //    item._Price = stage5.FirstOrDefault(_ => _.Id == item.Id)?.Price ?? 0f;
            //}
            //db.SaveChanges();
        }
    }
}
