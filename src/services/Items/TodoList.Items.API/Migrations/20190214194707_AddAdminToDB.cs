using Microsoft.EntityFrameworkCore.Migrations;

namespace TodoList.Items.API.Migrations
{
  public partial class AddAdminToDB : Migration
  {
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.Sql(
        @"
          INSERT INTO AspNetUsers (UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount)
          VALUES ('admin', 'ADMIN', 'admin@admin.admin', 'ADMIN@ADMIN.ADMIN', 1, 'AQAAAAEAACcQAAAAEPps9h221EpyXTDM/MlkZEt1+BFD6ACavmo1PAopG7Akv0nkO3I93kkCyMFUIj6UXw==', 'XYF25ONPIXEVSAPXYNLGN67KGIJOYR74', '61a9dbe1-72b2-46fb-b9c6-43c4f07bf54d', NULL, 0, 0, NULL, 1, 0);

          INSERT INTO AspNetUserRoles (UserId, RoleId)
          VALUES (1, 1);

          INSERT INTO AspNetUserRoles (UserId, RoleId)
          VALUES (1, 2);
        "
      );
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
  }
}
