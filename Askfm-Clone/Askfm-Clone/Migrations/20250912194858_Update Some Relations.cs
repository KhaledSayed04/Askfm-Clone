using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askfm_Clone.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSomeRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Users_CreatorId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Likes_AnswerId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "AnswerId",
                table: "QuestionRecipients");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Likes",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Follows",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Blocks",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_AnswerId_CreatedAt",
                table: "Likes",
                columns: new[] { "AnswerId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Users_CreatorId",
                table: "Answers",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Users_CreatorId",
                table: "Answers");

            migrationBuilder.DropIndex(
                name: "IX_Likes_AnswerId_CreatedAt",
                table: "Likes");

            migrationBuilder.AddColumn<int>(
                name: "AnswerId",
                table: "QuestionRecipients",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Likes",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Follows",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Blocks",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_AnswerId",
                table: "Likes",
                column: "AnswerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Users_CreatorId",
                table: "Answers",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
