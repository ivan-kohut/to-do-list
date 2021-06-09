using Microsoft.EntityFrameworkCore.Migrations;
using TodoList.Items.Domain.Aggregates.ItemAggregate;

namespace TodoList.Items.API.Migrations
{
  public partial class AddItemStatusesTable : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "ItemStatuses",
          columns: table => new
          {
            Id = table.Column<int>(type: "int", nullable: false)
                  .Annotation("SqlServer:Identity", "1, 1"),
            Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ItemStatuses", x => x.Id);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Items_Status",
          table: "Items",
          column: "Status");

      migrationBuilder.CreateIndex(
          name: "IX_ItemStatuses_Id",
          table: "ItemStatuses",
          column: "Id");

      migrationBuilder.Sql(
        $@"
          INSERT INTO ItemStatuses (Name) VALUES ('{ItemStatus.Todo.Name}');
          INSERT INTO ItemStatuses (Name) VALUES ('{ItemStatus.Done.Name}');
        "
      );

      migrationBuilder.AddForeignKey(
          name: "FK_Items_ItemStatuses_Status",
          table: "Items",
          column: "Status",
          principalTable: "ItemStatuses",
          principalColumn: "Id",
          onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropForeignKey(
          name: "FK_Items_ItemStatuses_Status",
          table: "Items");

      migrationBuilder.DropTable(
          name: "ItemStatuses");

      migrationBuilder.DropIndex(
          name: "IX_Items_Status",
          table: "Items");
    }
  }
}
