using Microsoft.EntityFrameworkCore.Migrations;

namespace MegaPOS.Migrations
{
    public partial class maxprice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "MaxPriceProcentage",
                table: "Products",
                type: "real",
                nullable: false,
                defaultValue: 3f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxPriceProcentage",
                table: "Products");
        }
    }
}
