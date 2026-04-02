using Microsoft.EntityFrameworkCore.Migrations;

namespace personal_website_api.Migrations
{
    public partial class SeedSystemUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "INSERT INTO \"Users\" (\"Id\", \"Name\", \"Email\", \"IsAdmin\") " +
                "VALUES (1, 'system', 'system@internal', false) " +
                "ON CONFLICT DO NOTHING;"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"Users\" WHERE \"Id\" = 1;");
        }
    }
}
