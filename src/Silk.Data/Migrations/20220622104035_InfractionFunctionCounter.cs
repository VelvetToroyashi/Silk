using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Silk.Data.Migrations
{
    public partial class InfractionFunctionCounter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                                 @"
CREATE OR REPLACE FUNCTION upsert_case_id() 
RETURNS TRIGGER AS
$$
BEGIN
-- Lock the row for update
LOCK TABLE infractions IN SHARE ROW EXCLUSIVE MODE;
-- If there's no infractions, MAX retruns NULL, so coalesce with 0
SELECT COALESCE(MAX(case_id), 0) + 1 INTO new.case_id FROM infractions WHERE guild_id=new.guild_id;

RETURN NEW; -- return the new row

END;
$$
LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS upsert_case ON infractions; -- Just in case this migration is run twice
CREATE TRIGGER upsert_case BEFORE INSERT ON infractions FOR EACH ROW EXECUTE PROCEDURE upsert_case_id();
");
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
