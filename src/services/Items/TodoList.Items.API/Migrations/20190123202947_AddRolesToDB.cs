using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoList.Items.API.Migrations
{
  public partial class AddRolesToDB : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
        @"
          INSERT INTO AspNetRoles (Name, NormalizedName) VALUES ('admin', 'ADMIN');
          INSERT INTO AspNetRoles (Name, NormalizedName) VALUES ('user', 'USER');
        "
      );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
  }
}
