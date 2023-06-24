using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KanbanApp.BackendServer.Data.Migrations
{
    public partial class RoleProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllRoles",
                table: "UserInProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RoleStatuses",
                table: "UserInProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoleStatusesName",
                table: "UserInProjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllRoles",
                table: "UserInProjects");

            migrationBuilder.DropColumn(
                name: "RoleStatuses",
                table: "UserInProjects");

            migrationBuilder.DropColumn(
                name: "RoleStatusesName",
                table: "UserInProjects");
        }
    }
}
