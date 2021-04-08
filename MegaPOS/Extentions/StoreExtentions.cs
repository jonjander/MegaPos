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


        

        public static void UpdateProductPrice(this DatabaseContext db, string storeId)
        {
            var store = db.Set<Store>()
                .Include(_ => _.Products)
                .ThenInclude(_ => _.Orders)
                .FirstOrDefault(_=>_.Id == storeId);

            var globalProfit = store.ProfitTarget;
            var totalProductsInStore = store.Orders.Where(_ => _.Type == OrderType.Assets).Count();
            var antalUnikaProdukter = store.Products.Count;

            var stage1 = store.Products.Select(_ => new {
                _,
                TotalNumber = _.Quantity + _.ProductsSold,
                NumberSold = _.ProductsSold,
                Quantity = _.Quantity
            }).Select(_ => new {
                _._,
                _.TotalNumber,
                _.NumberSold,
                _.Quantity,
                PercentageSold = _.NumberSold / _.TotalNumber,
                Popularity = _.NumberSold / totalProductsInStore
            }).ToList();

            var PercentageSoldTotal = stage1.Sum(_ => _.PercentageSold);
            var PopularityTotal = stage1.Sum(_ => _.Popularity);

            var stage2 = stage1.Select(_ => new
            {
                _._,
                _.TotalNumber,
                _.NumberSold,
                _.Quantity,
                _.PercentageSold,
                _.Popularity,
                h = _.PercentageSold / PercentageSoldTotal,
                hshift = (_.PercentageSold / PercentageSoldTotal) - (1 / antalUnikaProdukter),
                w = _.Popularity / PopularityTotal,
                wshift = (_.Popularity / PopularityTotal) - (1 / antalUnikaProdukter)
            }).ToList();

            var stage3 = stage2.Select(_=> new { 
                _._,
                _.TotalNumber,
                _.NumberSold,
                _.Quantity,
                _.PercentageSold,
                _.Popularity,
                _.h,
                _.hshift,
                _.w,
                _.wshift,
                npw = 1+_.wshift,
                nph = 1+_.hshift,
                npwh = (1 + _.wshift + 1 + _.hshift) / 2,
                hwshift = (_.hshift + _.wshift) / 2,
                PlannedProfit = _._.OriginalPrice * _.TotalNumber * (1+_._.LocalProfit),
                PlannedProfitNow = _._.OriginalPrice * _.NumberSold * (1+ _._.LocalProfit),
                Gains = _._.Orders.Where(_ => _.Type == OrderType.Revenues).Sum(_ => _.Credit)
            }).ToList();

            var stage4 = stage3.Select(_ => new {
                _._,
                _.TotalNumber,
                _.NumberSold,
                _.Quantity,
                _.PercentageSold,
                _.Popularity,
                _.h,
                _.hshift,
                _.w,
                _.wshift,
                _.npw,
                _.nph,
                _.npwh,
                _.hwshift,
                _.PlannedProfit,
                _.PlannedProfitNow,
                _.Gains,
                Margin = _.Gains - _.PlannedProfitNow,
                Left = _.PlannedProfit - _.Gains,
                LeftPerProduct = (_.PlannedProfit - _.Gains) / _.Quantity
            }).ToList();

            var totalMargin = stage4.Sum(_ => _.Margin);

            var stage5 = stage4.Select(_ => new
            {
                _._,
                NewPriceNoDiscount = _.Left * _.npwh * (1 + _._.LocalProfit),
                Discount = totalMargin * -(_.hwshift)
            }).Select(_ => new
            {
                _._,
                Price = _.NewPriceNoDiscount - _.Discount
            });

            foreach (var item in stage5)
            {
                item._.Price = item.Price;
            }
            db.SaveChanges();
        }
    }
}
