using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askfm_Clone.Migrations
{
    /// <inheritdoc />
    public partial class CleanUserSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAnonymousComments",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowAnonymousQuestions",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsAnonymousAccount",
                table: "Users",
                newName: "AllowAnonymous");

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "Users",
                newName: "ProfilePictureUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProfilePictureUrl",
                table: "Users",
                newName: "AvatarUrl");

            migrationBuilder.RenameColumn(
                name: "AllowAnonymous",
                table: "Users",
                newName: "IsAnonymousAccount");

            migrationBuilder.AddColumn<bool>(
                name: "AllowAnonymousComments",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowAnonymousQuestions",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
