using Microsoft.EntityFrameworkCore.Migrations;

namespace Repositories.Concrete.Migrations
{
  public partial class AddUserIdToItem : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<int>(
          name: "UserId",
          table: "Items",
          nullable: false,
          defaultValue: 0);

      migrationBuilder.CreateIndex(
          name: "IX_Items_UserId",
          table: "Items",
          column: "UserId");

      migrationBuilder.AddForeignKey(
          name: "FK_Items_AspNetUsers_UserId",
          table: "Items",
          column: "UserId",
          principalTable: "AspNetUsers",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Items_AspNetUsers_UserId",
          table: "Items");

      migrationBuilder.DropIndex(
          name: "IX_Items_UserId",
          table: "Items");

      migrationBuilder.DropColumn(
          name: "UserId",
          table: "Items");
    }
  }
}
