using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace personal_website_api.Migrations
{
    public partial class AddLastViewedAtToArticles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastViewedAt",
                table: "Articles",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastViewedAt",
                table: "Articles");
        }
    }
}
