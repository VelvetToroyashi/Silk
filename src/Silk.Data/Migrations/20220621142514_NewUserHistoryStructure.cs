using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class NewUserHistoryStructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE TABLE temp AS SELECT guild_id, user_id, leave_date FROM user_histories;");
            migrationBuilder.Sql("INSERT INTO temp SELECT guild_id, user_id, leave_date FROM user_histories h WHERE h.leave_date IS NOT NULL;");

            migrationBuilder.DropPrimaryKey(
                name: "PK_user_histories",
                table: "user_histories");
            
            migrationBuilder.DropColumn(
                name: "Id",
                table: "user_histories");

            migrationBuilder.DropColumn(
                name: "leave_date",
                table: "user_histories");

            migrationBuilder.RenameColumn(
                name: "join_date",
                table: "user_histories",
                newName: "date");
            
            migrationBuilder.AddColumn<bool>(
                name: "is_join",
                table: "user_histories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_histories",
                table: "user_histories",
                columns: new[] { "user_id", "guild_id", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_user_histories_guild_id",
                table: "user_histories",
                column: "guild_id");

            migrationBuilder.Sql("UPDATE user_histories SET is_join = true WHERE is_join IS FALSE;");
            
            migrationBuilder.Sql("INSERT INTO user_histories(guild_id, user_id, date, is_join) SELECT *, false FROM temp;");
            migrationBuilder.Sql("DROP TABLE temp;");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_histories",
                table: "user_histories");

            migrationBuilder.DropIndex(
                name: "IX_user_histories_guild_id",
                table: "user_histories");

            migrationBuilder.DropColumn(
                name: "is_join",
                table: "user_histories");

            migrationBuilder.RenameColumn(
                name: "date",
                table: "user_histories",
                newName: "join_date");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "user_histories",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "leave_date",
                table: "user_histories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_histories",
                table: "user_histories",
                column: "Id");
        }
    }
}
