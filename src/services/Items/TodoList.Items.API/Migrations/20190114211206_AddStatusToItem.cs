using Microsoft.EntityFrameworkCore.Migrations;
using TodoList.Items.Domain.Aggregates.ItemAggregate;

namespace TodoList.Items.API.Migrations
{
  public partial class AddStatusToItem : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<int>(
          name: "Status",
          table: "Items",
          nullable: false,
          defaultValue: ItemStatus.Todo.Id);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(
          name: "Status",
          table: "Items");
    }
  }
}
