using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askfm_Clone.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokenInfos_Users_UserId",
                table: "RefreshTokenInfos");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokenInfos",
                table: "RefreshTokenInfos");

            migrationBuilder.RenameTable(
                name: "RefreshTokenInfos",
                newName: "RefreshTokensInfo");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokenInfos_UserId",
                table: "RefreshTokensInfo",
                newName: "IX_RefreshTokensInfo_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokensInfo",
                table: "RefreshTokensInfo",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokensInfo_Users_UserId",
                table: "RefreshTokensInfo",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokensInfo_Users_UserId",
                table: "RefreshTokensInfo");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokensInfo",
                table: "RefreshTokensInfo");

            migrationBuilder.RenameTable(
                name: "RefreshTokensInfo",
                newName: "RefreshTokenInfos");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokensInfo_UserId",
                table: "RefreshTokenInfos",
                newName: "IX_RefreshTokenInfos_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokenInfos",
                table: "RefreshTokenInfos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokenInfos_Users_UserId",
                table: "RefreshTokenInfos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
