using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PDF.Migrations
{
    /// <inheritdoc />
    public partial class AddUsernameToApiCallLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Username",
                table: "ApiCallLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Username",
                table: "ApiCallLogs");
        }
    }
}
