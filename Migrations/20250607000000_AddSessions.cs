using Microsoft.EntityFrameworkCore.Migrations;

namespace personal_website_api.Migrations
{
    public partial class AddSessions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS ""Sessions"" (
                    ""Id"" text NOT NULL PRIMARY KEY,
                    ""UserId"" integer NOT NULL REFERENCES ""Users""(""Id"") ON DELETE CASCADE
                );
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS \"Sessions\";");
        }
    }
}
