using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MegaPOS.Model.Interfaces
{
    public interface IUnitOfWork
    {
        DbSet<Product> Products { get; set; }
        DbSet<Customer> Customers { get; set; }
        DbSet<Store> Stores { get; set; }
        DbSet<Order> Orders { get; set; }
        EntityEntry Add(object entity);
        EntityEntry<Product> AddProduct(Product entity);
        EntityEntry<Store> AddStore(Store entity);
        int SaveChanges();
    }
}
