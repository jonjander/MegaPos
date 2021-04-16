using Microsoft.EntityFrameworkCore.Migrations;

namespace MegaPOS.Migrations
{
#pragma warning disable IDE1006 // Naming Styles
    public partial class product2 : Migration
#pragma warning restore IDE1006 // Naming Styles
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MinPriceProcentage",
                table: "Product",
                type: "real",
                nullable: false,
                defaultValue: 0.9f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinPriceProcentage",
                table: "Product");
        }
    }
}
