using MegaPOS.Model;
using MegaPOS.Model.Events;
using MegaPOS.Model.vm;
using MegaPOS.Pages.Leaderboard;
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
                Quantity = _.Quantity
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
            => new OrderVm { 
                Price = order.Credit,
                ProductName = order.Product.Name
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

    }
}
