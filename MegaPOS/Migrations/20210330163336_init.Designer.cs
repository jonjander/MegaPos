// <auto-generated />
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
    [Migration("20210330163336_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.4")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MegaPOS.Model.Order", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<float>("Credit")
                        .HasColumnType("real");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<float>("Debit")
                        .HasColumnType("real");

                    b.Property<string>("ProductId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("StoreId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("StoreId");

                    b.ToTable("Order");
                });

            modelBuilder.Entity("MegaPOS.Model.Product", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<float>("Discount")
                        .HasColumnType("real");

                    b.Property<float>("LocalProfit")
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

                    b.Property<string>("StoreId1")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("StoreId");

                    b.HasIndex("StoreId1");

                    b.ToTable("Product");
                });

            modelBuilder.Entity("MegaPOS.Model.Store", b =>
                {
                    b.Property<string>("StoreId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<float>("ProfitTarget")
                        .HasColumnType("real");

                    b.HasKey("StoreId");

                    b.ToTable("Stores");
                });

            modelBuilder.Entity("MegaPOS.Model.Order", b =>
                {
                    b.HasOne("MegaPOS.Model.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId");

                    b.HasOne("MegaPOS.Model.Store", null)
                        .WithMany("Orders")
                        .HasForeignKey("StoreId");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("MegaPOS.Model.Product", b =>
                {
                    b.HasOne("MegaPOS.Model.Store", "Store")
                        .WithMany("Products")
                        .HasForeignKey("StoreId");

                    b.HasOne("MegaPOS.Model.Store", null)
                        .WithMany("AvalibleProducts")
                        .HasForeignKey("StoreId1");

                    b.Navigation("Store");
                });

            modelBuilder.Entity("MegaPOS.Model.Store", b =>
                {
                    b.Navigation("AvalibleProducts");

                    b.Navigation("Orders");

                    b.Navigation("Products");
                });
#pragma warning restore 612, 618
        }
    }
}
