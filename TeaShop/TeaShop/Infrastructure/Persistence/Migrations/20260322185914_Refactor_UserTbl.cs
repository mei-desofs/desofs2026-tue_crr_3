using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeaShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class Refactor_UserTbl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FirtName",
                table: "Users",
                newName: "FirstName");

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Active", "Address", "CreatedAt", "CreatedBy", "Email", "ExternalId", "FirstName", "LastName", "PhoneNumber", "UpdatedAt", "UpdatedBy" },
                values: new object[] { -1, true, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "seed", "admin@teashop.com", new Guid("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), "App", "Admin", null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: -1);

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Users",
                newName: "FirtName");

            migrationBuilder.AlterColumn<bool>(
                name: "Active",
                table: "Users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);
        }
    }
}
