using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyResource.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVideoModelDownloadUrl_04012025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DownloadUrl",
                table: "Video",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadUrl",
                table: "Video");
        }
    }
}
