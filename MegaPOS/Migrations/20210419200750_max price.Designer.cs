﻿// <auto-generated />
using System;
using MegaPOS.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MegaPOS.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20210419200750_max price")]
    partial class maxprice
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MegaPOS.Model.Customer", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool?>("Closed")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StoreId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("MegaPOS.Model.Order", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Created")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("GETDATE()");

                    b.Property<float>("Credit")
                        .HasColumnType("real");

                    b.Property<string>("CustomerId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<float>("Debit")
                        .HasColumnType("real");

                    b.Property<string>("ProductId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("StoreId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("ProductId");

                    b.HasIndex("StoreId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("MegaPOS.Model.Product", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("LocalProfit")
                        .HasColumnType("real");

                    b.Property<float>("MaxPriceProcentage")
                        .HasColumnType("real");

                    b.Property<float>("MinPriceProcentage")
                        .HasColumnType("real");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("OriginalPrice")
                        .HasColumnType("real");

                    b.Property<int>("ProductsSold")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<string>("StoreId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("MegaPOS.Model.Store", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PayoutSwishNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("ProfitTarget")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("Stores");
                });

            modelBuilder.Entity("MegaPOS.Model.Customer", b =>
                {
                    b.HasOne("MegaPOS.Model.Store", "Store")
                        .WithMany()
                        .HasForeignKey("StoreId");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("MegaPOS.Model.Order", b =>
                {
                    b.HasOne("MegaPOS.Model.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MegaPOS.Model.Product", "Product")
                        .WithMany("Orders")
                        .HasForeignKey("ProductId");

                    b.HasOne("MegaPOS.Model.Store", "Store")
                        .WithMany("Orders")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Customer");

                    b.Navigation("Product");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("MegaPOS.Model.Product", b =>
                {
                    b.HasOne("MegaPOS.Model.Store", "Store")
                        .WithMany("Products")
                        .HasForeignKey("StoreId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Store");
                });

            modelBuilder.Entity("MegaPOS.Model.Customer", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("MegaPOS.Model.Product", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("MegaPOS.Model.Store", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
