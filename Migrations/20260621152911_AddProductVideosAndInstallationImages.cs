using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace noithat_ducanh.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVideosAndInstallationImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstallationImageUrls",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VideoUrls",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstallationImageUrls",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "VideoUrls",
                table: "Products");
        }
    }
}
