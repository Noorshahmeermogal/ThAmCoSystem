using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ThAmCo.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AccountFunds = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    LastStockUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispatchedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSuppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SupplierProductId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSuppliers_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSuppliers_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Customers",
                columns: new[] { "Id", "AccountFunds", "CreatedAt", "DeliveryAddress", "Email", "IsActive", "Name", "Password", "PaymentAddress", "PhoneNumber", "UpdatedAt" },
                values: new object[] { 1, 100.00m, new DateTime(2025, 6, 9, 0, 25, 7, 670, DateTimeKind.Utc).AddTicks(6920), "123 Main St", "customer@example.com", true, "Test Customer", "$2a$11$ZnB56lWLuqXYE.xsd1oTrOX5MBIplRPsiBeHOxISZwWhSs2FSV7zC", null, "555-123-4567", new DateTime(2025, 6, 9, 0, 25, 7, 670, DateTimeKind.Utc).AddTicks(6920) });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "BasePrice", "Category", "CreatedAt", "CurrentPrice", "Description", "ImageUrl", "IsActive", "LastStockUpdate", "Name", "StockQuantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, 1200.00m, "Electronics", new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4795), 1320.00m, "Powerful laptop for all your needs.", "", true, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4794), "Laptop", 10, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4795) },
                    { 2, 800.00m, "Electronics", new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4804), 880.00m, "Latest model smartphone with great camera.", "", true, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4804), "Smartphone", 25, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4805) },
                    { 3, 10.00m, "Books", new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808), 11.00m, "Classic novel by F. Scott Fitzgerald.", "", true, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808), "The Great Gatsby", 50, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808) },
                    { 4, 150.00m, "Electronics", new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4811), 165.00m, "Immersive audio experience.", "", true, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4810), "Noise-Cancelling Headphones", 15, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4811) },
                    { 5, 8.00m, "Books", new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4814), 8.80m, "A comedic science fiction series.", "", true, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4813), "The Hitchhiker's Guide to the Galaxy", 40, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4814) }
                });

            migrationBuilder.InsertData(
                table: "Staff",
                columns: new[] { "Id", "CreatedAt", "Email", "IsActive", "Name", "Password", "Role", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 6, 9, 0, 25, 7, 497, DateTimeKind.Utc).AddTicks(7076), "staff@thamco.com", true, "Staff Admin", "$2a$11$bqbobRvLrTttI4m1AGn5OuMvAessd1lSQzboMEKNlHthWxYXKhCoW", "Staff", new DateTime(2025, 6, 9, 0, 25, 7, 497, DateTimeKind.Utc).AddTicks(7077) });

            migrationBuilder.InsertData(
                table: "Suppliers",
                columns: new[] { "Id", "ApiEndpoint", "ApiKey", "CreatedAt", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "http://localhost:5001/api/inventory", null, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4619), true, "Electronics Supplier" },
                    { 2, "http://localhost:5002/api/inventory", null, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4623), true, "Books Supplier" }
                });

            migrationBuilder.InsertData(
                table: "ProductSuppliers",
                columns: new[] { "Id", "LastUpdated", "ProductId", "StockQuantity", "SupplierId", "SupplierPrice", "SupplierProductId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4842), 1, 0, 1, 1200.00m, null },
                    { 2, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4848), 2, 0, 1, 800.00m, null },
                    { 3, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4850), 3, 0, 2, 10.00m, null },
                    { 4, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4851), 4, 0, 1, 150.00m, null },
                    { 5, new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4853), 5, 0, 2, 8.00m, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductId",
                table: "OrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerId",
                table: "Orders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber",
                table: "Orders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Category",
                table: "Products",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSuppliers_ProductId",
                table: "ProductSuppliers",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSuppliers_SupplierId",
                table: "ProductSuppliers",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_Email",
                table: "Staff",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerAuditLogs");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "ProductSuppliers");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
