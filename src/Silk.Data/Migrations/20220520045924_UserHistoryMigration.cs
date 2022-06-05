using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class UserHistoryMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_histories_user_id",
                table: "user_histories");

            migrationBuilder.DropColumn(
                name: "join_dates",
                table: "user_histories");

            migrationBuilder.DropColumn(
                name: "leave_dates",
                table: "user_histories");

            migrationBuilder.RenameColumn(
                name: "initial_join_date",
                table: "user_histories",
                newName: "join_date");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "leave_date",
                table: "user_histories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_histories_user_id",
                table: "user_histories",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_histories_user_id",
                table: "user_histories");

            migrationBuilder.DropColumn(
                name: "leave_date",
                table: "user_histories");

            migrationBuilder.RenameColumn(
                name: "join_date",
                table: "user_histories",
                newName: "initial_join_date");

            migrationBuilder.AddColumn<List<DateTimeOffset>>(
                name: "join_dates",
                table: "user_histories",
                type: "timestamp with time zone[]",
                nullable: false);

            migrationBuilder.AddColumn<List<DateTimeOffset>>(
                name: "leave_dates",
                table: "user_histories",
                type: "timestamp with time zone[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_user_histories_user_id",
                table: "user_histories",
                column: "user_id",
                unique: true);
        }
    }
}
