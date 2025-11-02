using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API_PDF.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiCallLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PdfGuid = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    HttpMethod = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RequestBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
                    ResponseBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiCallLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PdfHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PdfGuid = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OriginalUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    S3Url = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    S3BucketName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    S3ObjectKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LocalFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsStoredInS3 = table.Column<bool>(type: "bit", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    PageCount = table.Column<int>(type: "int", nullable: true),
                    Keywords = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Bookmarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsMerged = table.Column<bool>(type: "bit", nullable: false),
                    SourcePdfGuids = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PdfHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiCallLogs_ApplicationName",
                table: "ApiCallLogs",
                column: "ApplicationName");

            migrationBuilder.CreateIndex(
                name: "IX_ApiCallLogs_IsSuccess",
                table: "ApiCallLogs",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_ApiCallLogs_PdfGuid",
                table: "ApiCallLogs",
                column: "PdfGuid");

            migrationBuilder.CreateIndex(
                name: "IX_ApiCallLogs_PdfGuid_Timestamp",
                table: "ApiCallLogs",
                columns: new[] { "PdfGuid", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_ApiCallLogs_Timestamp",
                table: "ApiCallLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_ApplicationName",
                table: "PdfHistories",
                column: "ApplicationName");

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_CreatedAt",
                table: "PdfHistories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_IsDeleted",
                table: "PdfHistories",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_IsDeleted_PdfGuid",
                table: "PdfHistories",
                columns: new[] { "IsDeleted", "PdfGuid" });

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_IsMerged",
                table: "PdfHistories",
                column: "IsMerged");

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_IsStoredInS3",
                table: "PdfHistories",
                column: "IsStoredInS3");

            migrationBuilder.CreateIndex(
                name: "IX_PdfHistories_PdfGuid",
                table: "PdfHistories",
                column: "PdfGuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiCallLogs");

            migrationBuilder.DropTable(
                name: "PdfHistories");
        }
    }
}
