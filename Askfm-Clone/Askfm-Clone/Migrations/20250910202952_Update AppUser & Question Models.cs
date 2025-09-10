using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Askfm_Clone.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAppUserQuestionModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_FromUserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_Users_ToUserId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ToUserId",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ToUserId",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "FromUserId",
                table: "Comments",
                newName: "CreatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_FromUserId",
                table: "Comments",
                newName: "IX_Comments_CreatorId");

            migrationBuilder.AlterColumn<int>(
                name: "FromUserId",
                table: "Questions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CoinsTransactions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CoinsTransactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<int>(
                name: "ReceptorId",
                table: "Answers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "QuestionRecipients",
                columns: table => new
                {
                    ReceptorId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    AnswerId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionRecipients", x => new { x.QuestionId, x.ReceptorId });
                    table.ForeignKey(
                        name: "FK_QuestionRecipients_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionRecipients_Users_ReceptorId",
                        column: x => x.ReceptorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId_ReceptorId",
                table: "Answers",
                columns: new[] { "QuestionId", "ReceptorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionRecipients_ReceptorId",
                table: "QuestionRecipients",
                column: "ReceptorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_QuestionRecipients_QuestionId_ReceptorId",
                table: "Answers",
                columns: new[] { "QuestionId", "ReceptorId" },
                principalTable: "QuestionRecipients",
                principalColumns: new[] { "QuestionId", "ReceptorId" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_CreatorId",
                table: "Comments",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Answers_QuestionRecipients_QuestionId_ReceptorId",
                table: "Answers");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_CreatorId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "QuestionRecipients");

            migrationBuilder.DropIndex(
                name: "IX_Answers_QuestionId_ReceptorId",
                table: "Answers");

            migrationBuilder.DropColumn(
                name: "ReceptorId",
                table: "Answers");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Comments",
                newName: "FromUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_CreatorId",
                table: "Comments",
                newName: "IX_Comments_FromUserId");

            migrationBuilder.AlterColumn<int>(
                name: "FromUserId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ToUserId",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "CoinsTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "CoinsTransactions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ToUserId",
                table: "Questions",
                column: "ToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Answers_QuestionId",
                table: "Answers",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Answers_Questions_QuestionId",
                table: "Answers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_FromUserId",
                table: "Comments",
                column: "FromUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_Users_ToUserId",
                table: "Questions",
                column: "ToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
