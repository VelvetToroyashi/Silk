using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Silk.Data.Migrations
{
    public partial class Tags_Remove_TagAlias_Add_NullableTag_Original : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagAlias");

            migrationBuilder.AddColumn<int>(
                name: "OriginalTagId",
                table: "Tags",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_OriginalTagId",
                table: "Tags",
                column: "OriginalTagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_OriginalTagId",
                table: "Tags",
                column: "OriginalTagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_OriginalTagId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_OriginalTagId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "OriginalTagId",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "TagAlias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OrigionalTagId = table.Column<int>(type: "integer", nullable: false),
                    OwnerDatabaseId = table.Column<long>(type: "bigint", nullable: false),
                    OwnerId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagAlias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagAlias_Tags_OrigionalTagId",
                        column: x => x.OrigionalTagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagAlias_Users_OwnerDatabaseId",
                        column: x => x.OwnerDatabaseId,
                        principalTable: "Users",
                        principalColumn: "DatabaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagAlias_OrigionalTagId",
                table: "TagAlias",
                column: "OrigionalTagId");

            migrationBuilder.CreateIndex(
                name: "IX_TagAlias_OwnerDatabaseId",
                table: "TagAlias",
                column: "OwnerDatabaseId");
        }
    }
}
