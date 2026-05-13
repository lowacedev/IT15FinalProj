using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSMS.Migrations
{
    /// <inheritdoc />
    public partial class FixTicketCommentRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ticketcomments_ServiceRequests_ServiceRequestRequestId",
                table: "ticketcomments");

            migrationBuilder.DropIndex(
                name: "IX_ticketcomments_ServiceRequestRequestId",
                table: "ticketcomments");

            migrationBuilder.DropColumn(
                name: "ServiceRequestRequestId",
                table: "ticketcomments");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 568, DateTimeKind.Utc).AddTicks(9973));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 569, DateTimeKind.Utc).AddTicks(2733));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 569, DateTimeKind.Utc).AddTicks(2774));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 569, DateTimeKind.Utc).AddTicks(2779));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 569, DateTimeKind.Utc).AddTicks(2781));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 569, DateTimeKind.Utc).AddTicks(2784));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 310, DateTimeKind.Utc).AddTicks(3918));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 310, DateTimeKind.Utc).AddTicks(6854));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 310, DateTimeKind.Utc).AddTicks(6864));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 13, 37, 37, 310, DateTimeKind.Utc).AddTicks(6868));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: -1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 4, 13, 37, 37, 353, DateTimeKind.Utc).AddTicks(6283), "AQAAAAIAAYagAAAAECpna8TzbeXXhAV0Jl+BGZRRnABlZfIKiXVBnR+OQYN0MzX/D2/LV2ZHFWdEv76iuQ==", new DateTime(2026, 5, 4, 13, 37, 37, 353, DateTimeKind.Utc).AddTicks(6909) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceRequestRequestId",
                table: "ticketcomments",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 950, DateTimeKind.Utc).AddTicks(3644));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 950, DateTimeKind.Utc).AddTicks(6141));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 950, DateTimeKind.Utc).AddTicks(6151));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 950, DateTimeKind.Utc).AddTicks(6155));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 950, DateTimeKind.Utc).AddTicks(6158));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "CategoryId",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 950, DateTimeKind.Utc).AddTicks(6160));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 706, DateTimeKind.Utc).AddTicks(1744));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 706, DateTimeKind.Utc).AddTicks(4669));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 706, DateTimeKind.Utc).AddTicks(4678));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 11, 41, 54, 706, DateTimeKind.Utc).AddTicks(4682));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: -1,
                columns: new[] { "CreatedAt", "PasswordHash", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 5, 4, 11, 41, 54, 743, DateTimeKind.Utc).AddTicks(7712), "AQAAAAIAAYagAAAAEO/hhjZHBR4IqV1taqdpFDzs5YurJUG9dwxFgvDlc8dHqHXMECgSUk8pwiDh1tl6ig==", new DateTime(2026, 5, 4, 11, 41, 54, 743, DateTimeKind.Utc).AddTicks(8622) });

            migrationBuilder.CreateIndex(
                name: "IX_ticketcomments_ServiceRequestRequestId",
                table: "ticketcomments",
                column: "ServiceRequestRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_ticketcomments_ServiceRequests_ServiceRequestRequestId",
                table: "ticketcomments",
                column: "ServiceRequestRequestId",
                principalTable: "ServiceRequests",
                principalColumn: "RequestId");
        }
    }
}
