using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EDU_TEST.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 17, 4, 44, 839, DateTimeKind.Utc).AddTicks(7342));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 17, 4, 44, 839, DateTimeKind.Utc).AddTicks(7346));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "LastName", "PasswordHash", "Role" },
                values: new object[] { 4, new DateTime(2026, 2, 25, 17, 4, 44, 839, DateTimeKind.Utc).AddTicks(7349), "tom@gmail.com", "Tom", "Admin", "123456", "Admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 17, 0, 45, 768, DateTimeKind.Utc).AddTicks(7774));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 2, 25, 17, 0, 45, 768, DateTimeKind.Utc).AddTicks(7777));
        }
    }
}
