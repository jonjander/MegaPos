using Microsoft.EntityFrameworkCore.Migrations;

namespace MegaPOS.Migrations
{
    public partial class dbfix0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Stores_StoreId1",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_StoreId1",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StoreId1",
                table: "Product");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreId1",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_StoreId1",
                table: "Product",
                column: "StoreId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Stores_StoreId1",
                table: "Product",
                column: "StoreId1",
                principalTable: "Stores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
