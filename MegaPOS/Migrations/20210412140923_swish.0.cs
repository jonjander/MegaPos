using Microsoft.EntityFrameworkCore.Migrations;

namespace MegaPOS.Migrations
{
    public partial class swish0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PayoutSwishNumber",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PayoutSwishNumber",
                table: "Stores");
        }
    }
}
