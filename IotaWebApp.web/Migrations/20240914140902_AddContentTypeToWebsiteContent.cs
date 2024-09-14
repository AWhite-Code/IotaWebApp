using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IotaWebApp.web.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypeToWebsiteContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "WebsiteContents",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "WebsiteContents");
        }
    }
}
