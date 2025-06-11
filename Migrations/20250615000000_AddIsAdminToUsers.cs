using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace personal_website_api.Migrations
{
    public partial class AddIsAdminToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE \"Users\"
                ADD COLUMN IF NOT EXISTS \"IsAdmin\" boolean NOT NULL DEFAULT FALSE;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE \"Users\"
                DROP COLUMN IF EXISTS \"IsAdmin\";
            ");
        }
    }
}
