using Azure.Core;
using Azure.Identity;
using MegaPOS.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaPOS.DBContext
{
    public class DatabaseContext : DbContext
    {
        private readonly DefaultAzureCredential azureSqlAuthTokenService;
        private readonly IConfiguration configuration;
        public DatabaseContext(
            IConfiguration configuration,
            DbContextOptions<DatabaseContext> options) : base(options)
        {
            azureSqlAuthTokenService = new DefaultAzureCredential();
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (configuration != null)
            {
                var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
                var tokenRequestContext = new TokenRequestContext(new[] { "https://database.windows.net//.default" });

                if (azureSqlAuthTokenService != null && configuration.GetValue<bool>("UseMSI"))
                    connection.AccessToken = azureSqlAuthTokenService.GetToken(tokenRequestContext, default).Token;

                optionsBuilder.UseSqlServer(connection);
            }
        }

        public DbSet<Store> Stores { get; set; }
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //order
            modelBuilder.Entity<Order>()
                .HasKey(_ => _.Id);

            modelBuilder.Entity<Order>()
                .Property(_ => _.Id)
                .ValueGeneratedOnAdd();
                
            modelBuilder.Entity<Order>()
                .Property(_ => _.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Order>()
                .HasOne(_ => _.Product)
                .WithMany(_=>_.Orders)
                .HasForeignKey(_=>_.ProductId);


            //store
            modelBuilder.Entity<Store>()
                .HasKey(_ => _.Id);

            modelBuilder.Entity<Store>()
                .Property(_ => _.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Store>()
                .HasMany(_ => _.Orders)
                .WithOne(_=> _.Store)
                .HasForeignKey(_=>_.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Store>()
                .HasMany(_ => _.Products)
                .WithOne(_ => _.Store)
                .HasForeignKey(_ => _.StoreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Store>()
                .HasMany<Customer>()
                .WithOne(_ => _.Store)
                .HasForeignKey(_ => _.StoreId);


            //Order
            modelBuilder.Entity<Order>()
                .HasKey(_ => _.Id);

            modelBuilder.Entity<Order>()
                .Property(_ => _.Id)
                .ValueGeneratedOnAdd();


            //Customer
            modelBuilder.Entity<Customer>()
                .HasKey(_ => _.Id);

            modelBuilder.Entity<Customer>()
                .Property(_ => _.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Customer>()
               .HasMany(_ => _.Orders)
               .WithOne(_=>_.Customer)
               .HasForeignKey(_=>_.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
