using MegaPOS.DBContext;
using MegaPOS.Enum;
using MegaPOS.Model;
using MegaPOS.Model.Events;
using MegaPOS.Model.vm;
using MegaPOS.Pages.LeaderboardPage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MegaPOS.Extentions
{
    public static class StoreExtentions
    {
        public static LeaderboardRowModel ToLeaderboardModel(this ProductVm _)
        => new()
        {
                Id = _.Id,
                Order = 0,
                Price = _.Price,
                Name = _.Name,
                ProductId = _.Id
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

        public static List<LeaderboardRowModel> UpdatePrice(this List<LeaderboardRowModel> rows, PriceChangeEvent updateEvent, float newPrice)
        {
            var rowToUpdate = rows.FirstOrDefault(_ => _.ProductId == updateEvent.ProductId);
            if (rowToUpdate != null)
            {
                rowToUpdate.Price = newPrice;
            }
            return rows;
        }

        public static List<ProductVm> UpdatePrice(this List<ProductVm> rows, PriceChangeEvent updateEvent, float newPrice)
        {
            var rowToUpdate = rows.FirstOrDefault(_ => _.Id == updateEvent.ProductId);
            if (rowToUpdate != null)
            {
                rowToUpdate.Price = newPrice;
                rowToUpdate.Quantity = updateEvent.NewQuantity;
            }
            return rows;
        }

        public static ProductVm ToVm(this Product _)
            => new()
            {
                Price = _.Price,
                Name = _.Name,
                Id = _.Id,
                Quantity = _.Quantity,
                LocalProfit = _.LocalProfit,
                MinPriceProcentage = _.MinPriceProcentage,
                MaxPriceProcentage = _.MaxPriceProcentage,
                Color = _.Color
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
                Id = order.Id,
                Created = order.Created
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
            => new()
            { Object = obj };

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
            float globalProfit = store.ProfitTarget;
            float totalProductsInStore = store.Orders.Where(_ => _.Type == OrderType.Assets).Sum(_=>_.Quantity);
            float numberOfUniqueProducts = store.Products.Count;

            var products = store.Products
                .Distinct(new ProductComparer())
                .ToList();
            var stage1 = products
                .Select(_ => new
                {
                    _.Id,
                    _.MinPriceProcentage,
                    _.MaxPriceProcentage,
                    _.OriginalPrice,
                    _.Quantity,
                    LocalProfit = (_.LocalProfit + globalProfit) / 2f,
                    _.Orders,
                    TotalNumber = (float)_.Quantity + _.ProductsSold,
                    NumberSold = _.ProductsSold,
                }).Select(_ => new
                {
                    _.Id,
                    _.MinPriceProcentage,
                    _.MaxPriceProcentage,
                    _.OriginalPrice,
                    _.Quantity,
                    _.LocalProfit,
                    _.TotalNumber,
                    _.NumberSold,
                    _.Orders,
                    PercentageSold = _.TotalNumber == 0f ? 0f : _.NumberSold / (float)_.TotalNumber,
                    Popularity = totalProductsInStore == 0f ? 0f : _.NumberSold / (float)totalProductsInStore
                }).ToList();

            var keys = stage1.OrderBy(_ => _.Id).Select(_ => $"{_.NumberSold}{_.LocalProfit}{_.TotalNumber}{_.MinPriceProcentage}{_.MaxPriceProcentage}");
            var statKey = string.Join("", keys);
            var cahcekey = $"{statKey}{globalProfit}{totalProductsInStore}{numberOfUniqueProducts}";

            using var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(cahcekey);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            var cahcekeyHash = sb.ToString();
            
          if (!Global.memoryCache.TryGetValue(cahcekeyHash, out List<PriceCalc> stage3))
            {
                float PercentageSoldTotal = stage1.Sum(_ => _.PercentageSold);
                float PopularityTotal = stage1.Sum(_ => _.Popularity);

                var stage2 = stage1.Select(_ => new
                {
                    _.Id,
                    _.MinPriceProcentage,
                    _.MaxPriceProcentage,
                    _.OriginalPrice,
                    Quantity = (float)_.Quantity,
                    _.LocalProfit,
                    TotalNumber = (float)_.TotalNumber,
                    NumberSold = (float)_.NumberSold,
                    _.Orders,
                    hshift = (PercentageSoldTotal == 0 ? 0 : _.PercentageSold / PercentageSoldTotal) - (1f / numberOfUniqueProducts),
                    wshift = (PopularityTotal == 0 ? 0 : _.Popularity / PopularityTotal) - (1f / numberOfUniqueProducts)
                }).Select(_ => new
                {
                    _.Id,
                    _.MinPriceProcentage,
                    _.MaxPriceProcentage,
                    _.OriginalPrice,
                    _.Quantity,
                    _.LocalProfit,
                    npwh = (2f + _.wshift + _.hshift) / 2f,
                    hwshift = (_.hshift + _.wshift) / 2f,
                    PlannedProfit = _.OriginalPrice * _.TotalNumber * _.LocalProfit,
                    PlannedProfitNow = _.OriginalPrice * _.NumberSold * _.LocalProfit,
                    Gains = _.Orders.Where(_ => _.Type == OrderType.Revenues).Sum(_ => _.Credit)
                }).Select(_ => new {
                    _.Id,
                    _.MinPriceProcentage,
                    _.MaxPriceProcentage,
                    _.OriginalPrice,
                    _.Quantity,
                    _.npwh,
                    _.hwshift,
                    _.LocalProfit,
                    Margin = _.Gains - _.PlannedProfitNow,
                    Left = _.PlannedProfit - _.Gains,
                    LeftPerProduct = (_.PlannedProfit - _.Gains) / _.Quantity
                }).ToList();

                float totalMargin = stage2.Sum(_ => _.Margin);

                stage3 = stage2.Select(_ => new
                {
                    _.Id,
                    _.MinPriceProcentage,
                    _.MaxPriceProcentage,
                    _.Quantity,
                    _.OriginalPrice,
                    NewPriceNoDiscount = _.LeftPerProduct * _.npwh * _.LocalProfit,
                    Discount = totalMargin * -_.hwshift
                }).Select(_ => new PriceCalc
                {
                    Id = _.Id,
                    Quantity = _.Quantity,
                    MinPriceProcentage = _.MinPriceProcentage,
                    MaxPriceProcentage = _.MaxPriceProcentage,
                    OriginalPrice = _.OriginalPrice,
                    Price = _.NewPriceNoDiscount - _.Discount
                }).ToList();

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromMinutes(1));
                Global.memoryCache.Set(cahcekeyHash, stage3, cacheEntryOptions);
            } else
            {

            }
            
            var updateProducts = products.ToList();
            var slectedProduct = stage3.FirstOrDefault(_ => _.Id == productId);
            var priceFactor = 1f;

            if (slectedProduct.Quantity <= 5)
                priceFactor += 0.3f * slectedProduct.Quantity;

            var calculatedPrice = slectedProduct?.Price * priceFactor ?? 0f;
            var minPrice = slectedProduct.OriginalPrice * slectedProduct.MinPriceProcentage;
            var maxPrice = slectedProduct.OriginalPrice * slectedProduct.MaxPriceProcentage;
            if (maxPrice < minPrice)
                maxPrice = minPrice;

            if (calculatedPrice < minPrice)
                calculatedPrice = minPrice;
            if (calculatedPrice > maxPrice)
                return maxPrice;
            return calculatedPrice;

        }
    }
}
