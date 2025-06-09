using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThAmCo.WebApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountFunds", "CreatedAt", "DeliveryAddress", "Password", "PhoneNumber", "UpdatedAt" },
                values: new object[] { 5000000.00m, new DateTime(2025, 6, 9, 1, 6, 13, 268, DateTimeKind.Utc).AddTicks(7697), "123 Customer St, Custville", "$2a$11$2a3jnip7e.WHsSnmG7N4A.nrt6Rrart6H/r9l6YrhiNrYzWQB41Yq", "+1234567890", new DateTime(2025, 6, 9, 1, 6, 13, 268, DateTimeKind.Utc).AddTicks(7697) });

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2517));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2519));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2521));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2522));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2523));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2474), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2473), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2474) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2484), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2483), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2484) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2487), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2486), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2487) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2489), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2489), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2489) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2491), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2491), new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2491) });

            migrationBuilder.UpdateData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 1, 6, 13, 131, DateTimeKind.Utc).AddTicks(8127), "$2a$11$HL8U9.XK6LePJyPZSiez7OjcvIAGiNWGzBsNQlRd5lbzR6Ll8uwzC", new DateTime(2025, 6, 9, 1, 6, 13, 131, DateTimeKind.Utc).AddTicks(8134) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2241));

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 9, 1, 6, 13, 4, DateTimeKind.Utc).AddTicks(2246));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Customers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AccountFunds", "CreatedAt", "DeliveryAddress", "Password", "PhoneNumber", "UpdatedAt" },
                values: new object[] { 100.00m, new DateTime(2025, 6, 9, 0, 25, 7, 670, DateTimeKind.Utc).AddTicks(6920), "123 Main St", "$2a$11$ZnB56lWLuqXYE.xsd1oTrOX5MBIplRPsiBeHOxISZwWhSs2FSV7zC", "555-123-4567", new DateTime(2025, 6, 9, 0, 25, 7, 670, DateTimeKind.Utc).AddTicks(6920) });

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 1,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4842));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 2,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4848));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 3,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4850));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 4,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4851));

            migrationBuilder.UpdateData(
                table: "ProductSuppliers",
                keyColumn: "Id",
                keyValue: 5,
                column: "LastUpdated",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4853));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4795), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4794), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4795) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4804), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4804), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4805) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4808) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4811), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4810), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4811) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "LastStockUpdate", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4814), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4813), new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4814) });

            migrationBuilder.UpdateData(
                table: "Staff",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "Password", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 6, 9, 0, 25, 7, 497, DateTimeKind.Utc).AddTicks(7076), "$2a$11$bqbobRvLrTttI4m1AGn5OuMvAessd1lSQzboMEKNlHthWxYXKhCoW", new DateTime(2025, 6, 9, 0, 25, 7, 497, DateTimeKind.Utc).AddTicks(7077) });

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4619));

            migrationBuilder.UpdateData(
                table: "Suppliers",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 6, 9, 0, 25, 7, 338, DateTimeKind.Utc).AddTicks(4623));
        }
    }
}
