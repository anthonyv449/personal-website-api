using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personal_website_api.Migrations
{
    public partial class AddEmailToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add the Email column only if it doesn't already exist
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users""
                ADD COLUMN IF NOT EXISTS ""Email"" text;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the Email column only if it exists
            migrationBuilder.Sql(@"
                ALTER TABLE ""Users""
                DROP COLUMN IF EXISTS ""Email"";
            ");
        }
    }
}
