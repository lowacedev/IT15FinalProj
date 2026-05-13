using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSMS.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ticketcomments",
                columns: table => new
                {
                    CommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    Body = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsInternal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ServiceRequestRequestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticketcomments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_ticketcomments_ServiceRequests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ticketcomments_ServiceRequests_ServiceRequestRequestId",
                        column: x => x.ServiceRequestRequestId,
                        principalTable: "ServiceRequests",
                        principalColumn: "RequestId");
                    table.ForeignKey(
                        name: "FK_ticketcomments_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "IX_ticketcomments_AuthorId",
                table: "ticketcomments",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ticketcomments_RequestId",
                table: "ticketcomments",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ticketcomments_ServiceRequestRequestId",
                table: "ticketcomments",
                column: "ServiceRequestRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ticketcomments");

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
                column: "CreatedAt",
                value: new DateTime(2026, 5, 4, 7, 57, 8, 485, DateTimeKind.Utc).AddTicks(2456));

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
    }
}
