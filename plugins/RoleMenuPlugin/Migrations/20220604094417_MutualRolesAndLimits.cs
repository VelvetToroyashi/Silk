using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoleMenuPlugin.Migrations
{
    public partial class MutualRolesAndLimits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComponentId",
                table: "RoleMenuOptionModel");

            migrationBuilder.AddColumn<int>(
                name: "MaxSelections",
                table: "RoleMenus",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal[]>(
                name: "MutuallyExclusiveRoleIds",
                table: "RoleMenuOptionModel",
                type: "numeric(20,0)[]",
                nullable: false,
                defaultValue: new decimal[0]);

            migrationBuilder.AddColumn<decimal[]>(
                name: "MutuallyInclusiveRoleIds",
                table: "RoleMenuOptionModel",
                type: "numeric(20,0)[]",
                nullable: false,
                defaultValue: new decimal[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxSelections",
                table: "RoleMenus");

            migrationBuilder.DropColumn(
                name: "MutuallyExclusiveRoleIds",
                table: "RoleMenuOptionModel");

            migrationBuilder.DropColumn(
                name: "MutuallyInclusiveRoleIds",
                table: "RoleMenuOptionModel");

            migrationBuilder.AddColumn<string>(
                name: "ComponentId",
                table: "RoleMenuOptionModel",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
