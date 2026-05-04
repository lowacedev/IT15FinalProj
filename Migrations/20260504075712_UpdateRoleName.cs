using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRoleName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 783, DateTimeKind.Utc).AddTicks(7321));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 784, DateTimeKind.Utc).AddTicks(764));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 784, DateTimeKind.Utc).AddTicks(780));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 784, DateTimeKind.Utc).AddTicks(784));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 784, DateTimeKind.Utc).AddTicks(786));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 784, DateTimeKind.Utc).AddTicks(789));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 484, DateTimeKind.Utc).AddTicks(9297));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 485, DateTimeKind.Utc).AddTicks(2445));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "RoleName" },
                values: new object[] { new DateTime(2026, 5, 4, 7, 57, 8, 485, DateTimeKind.Utc).AddTicks(2456), "Employee / Requestor", "Employee" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 485, DateTimeKind.Utc).AddTicks(2459));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: -1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 4, 7, 57, 8, 532, DateTimeKind.Utc).AddTicks(628), "AQAAAAIAAYagAAAAEA31Wt1vhrOv8fcB/Lcgk3Fi+nnYER2GJj3V/2imlPavmBknpzg99V55fb+TaKYADw==", new DateTime(2026, 5, 4, 7, 57, 8, 532, DateTimeKind.Utc).AddTicks(1300) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 13, 186, DateTimeKind.Utc).AddTicks(4220));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 13, 186, DateTimeKind.Utc).AddTicks(6766));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 13, 186, DateTimeKind.Utc).AddTicks(6777));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 13, 186, DateTimeKind.Utc).AddTicks(6781));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 13, 186, DateTimeKind.Utc).AddTicks(6783));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 13, 186, DateTimeKind.Utc).AddTicks(6786));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 12, 888, DateTimeKind.Utc).AddTicks(6469));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 12, 889, DateTimeKind.Utc).AddTicks(654));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                columns: new[] { "CreatedAt", "Description", "RoleName" },
                values: new object[] { new DateTime(2026, 5, 4, 7, 35, 12, 889, DateTimeKind.Utc).AddTicks(671), "Employee / Client / Requestor", "Client" });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 35, 12, 889, DateTimeKind.Utc).AddTicks(674));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: -1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 4, 7, 35, 12, 950, DateTimeKind.Utc).AddTicks(5155), "AQAAAAIAAYagAAAAEK9NgXj5QNm6UkFBO4Doeh8Iocl1jlqXSHFXAqxCc7DqtKdhH4v17woNqr2CwvaGVA==", new DateTime(2026, 5, 4, 7, 35, 12, 950, DateTimeKind.Utc).AddTicks(5861) });
        }
    }
}
