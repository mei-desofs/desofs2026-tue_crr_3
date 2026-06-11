using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeaShop.Migrations
{
    /// <inheritdoc />
    public partial class UpdateImageToOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Teas",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Teas",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFilePath",
                table: "Teas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ImageSizeBytes",
                table: "Teas",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ImageUploadedAt",
                table: "Teas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Image_Id",
                table: "Teas",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Teas");

            migrationBuilder.DropColumn(
                name: "ImageFilePath",
                table: "Teas");

            migrationBuilder.DropColumn(
                name: "ImageSizeBytes",
                table: "Teas");

            migrationBuilder.DropColumn(
                name: "ImageUploadedAt",
                table: "Teas");

            migrationBuilder.DropColumn(
                name: "Image_Id",
                table: "Teas");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Teas",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);
        }
    }
}
