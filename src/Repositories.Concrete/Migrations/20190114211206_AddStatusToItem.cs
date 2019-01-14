using Entities;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Repositories.Concrete.Migrations
{
  public partial class AddStatusToItem : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<int>(
          name: "Status",
          table: "Items",
          nullable: false,
          defaultValue: ItemStatus.Todo);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "Status",
          table: "Items");
    }
  }
}
