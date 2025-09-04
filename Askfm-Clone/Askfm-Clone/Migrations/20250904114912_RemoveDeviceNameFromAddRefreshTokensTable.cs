using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askfm_Clone.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeviceNameFromAddRefreshTokensTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "RefreshTokenInfos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "RefreshTokenInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
