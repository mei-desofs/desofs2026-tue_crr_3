using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeaShop.Migrations
{
    /// <inheritdoc />
    public partial class HashSessionTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_Token",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "TokenHash",
                table: "Sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_TokenHash",
                table: "Sessions",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessions_TokenHash",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "TokenHash",
                table: "Sessions");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "Sessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Token",
                table: "Sessions",
                column: "Token",
                unique: true);
        }
    }
}
