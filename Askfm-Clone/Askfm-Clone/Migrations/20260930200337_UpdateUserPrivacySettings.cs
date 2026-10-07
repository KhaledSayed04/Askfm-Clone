using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askfm_Clone.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserPrivacySettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_FromUserId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AnswerId",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "AllowAnonymous",
                table: "Users",
                newName: "IsAnonymousAccount");

            migrationBuilder.RenameColumn(
                name: "FromUserId",
                table: "Questions",
                newName: "SenderId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_FromUserId",
                table: "Questions",
                newName: "IX_Questions_SenderId");

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

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "CoinsTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AnswerId_CreatedAt",
                table: "Comments",
                columns: new[] { "AnswerId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_SenderId",
                table: "Questions",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_SenderId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_AnswerId_CreatedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AllowAnonymousComments",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowAnonymousQuestions",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "IsAnonymousAccount",
                table: "Users",
                newName: "AllowAnonymous");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                table: "Questions",
                newName: "FromUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Questions_SenderId",
                table: "Questions",
                newName: "IX_Questions_FromUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CoinsTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AnswerId",
                table: "Comments",
                column: "AnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_FromUserId",
                table: "Questions",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
