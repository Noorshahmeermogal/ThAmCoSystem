﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ThAmCo.WebApi.Data;

#nullable disable

namespace ThAmCo.WebApi.Migrations
{
    [DbContext(typeof(ThAmCoContext))]
    [Migration("20250609002508_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ThAmCo.WebApi.Models.Customer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("AccountFunds")
                        .HasColumnType("decimal(10,2)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("DeliveryAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("PaymentAddress")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Customers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            AccountFunds = 100.00m,
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 670, DateTimeKind.Utc).AddTicks(6920),
                            DeliveryAddress = "123 Main St",
                            Email = "customer@example.com",
                            IsActive = true,
                            Name = "Test Customer",
                            Password = "$2a$11$ZnB56lWLuqXYE.xsd1oTrOX5MBIplRPsiBeHOxISZwWhSs2FSV7zC",
                            PhoneNumber = "555-123-4567",
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 670, DateTimeKind.Utc).AddTicks(6920)
                        });
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.CustomerAuditLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("ChangedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("ChangedBy")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("NewValues")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OldValues")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CustomerAuditLogs");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("DeliveryAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DispatchedDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OrderNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.HasIndex("OrderNumber")
                        .IsUnique();

                    b.HasIndex("Status");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.OrderItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("Quantity")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("decimal(10,2)");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderItems");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("BasePrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Category")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("CurrentPrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ImageUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LastStockUpdate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("StockQuantity")
                        .HasColumnType("int");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Category");

                    b.HasIndex("Name");

                    b.ToTable("Products");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            BasePrice = 1200.00m,
                            Category = "Electronics",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4795),
                            CurrentPrice = 1320.00m,
                            Description = "Powerful laptop for all your needs.",
                            ImageUrl = "",
                            IsActive = true,
                            LastStockUpdate = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4794),
                            Name = "Laptop",
                            StockQuantity = 10,
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4795)
                        },
                        new
                        {
                            Id = 2,
                            BasePrice = 800.00m,
                            Category = "Electronics",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4804),
                            CurrentPrice = 880.00m,
                            Description = "Latest model smartphone with great camera.",
                            ImageUrl = "",
                            IsActive = true,
                            LastStockUpdate = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4804),
                            Name = "Smartphone",
                            StockQuantity = 25,
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4805)
                        },
                        new
                        {
                            Id = 3,
                            BasePrice = 10.00m,
                            Category = "Books",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808),
                            CurrentPrice = 11.00m,
                            Description = "Classic novel by F. Scott Fitzgerald.",
                            ImageUrl = "",
                            IsActive = true,
                            LastStockUpdate = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808),
                            Name = "The Great Gatsby",
                            StockQuantity = 50,
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808)
                        },
                        new
                        {
                            Id = 4,
                            BasePrice = 150.00m,
                            Category = "Electronics",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4811),
                            CurrentPrice = 165.00m,
                            Description = "Immersive audio experience.",
                            ImageUrl = "",
                            IsActive = true,
                            LastStockUpdate = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4810),
                            Name = "Noise-Cancelling Headphones",
                            StockQuantity = 15,
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4811)
                        },
                        new
                        {
                            Id = 5,
                            BasePrice = 8.00m,
                            Category = "Books",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4814),
                            CurrentPrice = 8.80m,
                            Description = "A comedic science fiction series.",
                            ImageUrl = "",
                            IsActive = true,
                            LastStockUpdate = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4813),
                            Name = "The Hitchhiker's Guide to the Galaxy",
                            StockQuantity = 40,
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4814)
                        });
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.ProductSupplier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<int>("StockQuantity")
                        .HasColumnType("int");

                    b.Property<int>("SupplierId")
                        .HasColumnType("int");

                    b.Property<decimal>("SupplierPrice")
                        .HasColumnType("decimal(10,2)");

                    b.Property<string>("SupplierProductId")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("SupplierId");

                    b.ToTable("ProductSuppliers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            LastUpdated = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4842),
                            ProductId = 1,
                            StockQuantity = 0,
                            SupplierId = 1,
                            SupplierPrice = 1200.00m
                        },
                        new
                        {
                            Id = 2,
                            LastUpdated = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4848),
                            ProductId = 2,
                            StockQuantity = 0,
                            SupplierId = 1,
                            SupplierPrice = 800.00m
                        },
                        new
                        {
                            Id = 3,
                            LastUpdated = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4850),
                            ProductId = 3,
                            StockQuantity = 0,
                            SupplierId = 2,
                            SupplierPrice = 10.00m
                        },
                        new
                        {
                            Id = 4,
                            LastUpdated = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4851),
                            ProductId = 4,
                            StockQuantity = 0,
                            SupplierId = 1,
                            SupplierPrice = 150.00m
                        },
                        new
                        {
                            Id = 5,
                            LastUpdated = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4853),
                            ProductId = 5,
                            StockQuantity = 0,
                            SupplierId = 2,
                            SupplierPrice = 8.00m
                        });
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Staff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Staff");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 497, DateTimeKind.Utc).AddTicks(7076),
                            Email = "staff@thamco.com",
                            IsActive = true,
                            Name = "Staff Admin",
                            Password = "$2a$11$bqbobRvLrTttI4m1AGn5OuMvAessd1lSQzboMEKNlHthWxYXKhCoW",
                            Role = "Staff",
                            UpdatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 497, DateTimeKind.Utc).AddTicks(7077)
                        });
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Supplier", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ApiEndpoint")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("ApiKey")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.ToTable("Suppliers");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ApiEndpoint = "http://localhost:5001/api/inventory",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4619),
                            IsActive = true,
                            Name = "Electronics Supplier"
                        },
                        new
                        {
                            Id = 2,
                            ApiEndpoint = "http://localhost:5002/api/inventory",
                            CreatedAt = new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4623),
                            IsActive = true,
                            Name = "Books Supplier"
                        });
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Order", b =>
                {
                    b.HasOne("ThAmCo.WebApi.Models.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.OrderItem", b =>
                {
                    b.HasOne("ThAmCo.WebApi.Models.Order", "Order")
                        .WithMany("OrderItems")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ThAmCo.WebApi.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.ProductSupplier", b =>
                {
                    b.HasOne("ThAmCo.WebApi.Models.Product", "Product")
                        .WithMany("ProductSuppliers")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ThAmCo.WebApi.Models.Supplier", "Supplier")
                        .WithMany()
                        .HasForeignKey("SupplierId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Supplier");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Customer", b =>
                {
                    b.Navigation("Orders");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Order", b =>
                {
                    b.Navigation("OrderItems");
                });

            modelBuilder.Entity("ThAmCo.WebApi.Models.Product", b =>
                {
                    b.Navigation("ProductSuppliers");
                });
#pragma warning restore 612, 618
        }
    }
}
