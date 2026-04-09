using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LMS.Infractructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Feedback",
                table: "Submissions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FeedbackGivenAt",
                table: "Submissions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeedbackGivenByTeacherId",
                table: "Submissions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_FeedbackGivenByTeacherId",
                table: "Submissions",
                column: "FeedbackGivenByTeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_AspNetUsers_FeedbackGivenByTeacherId",
                table: "Submissions",
                column: "FeedbackGivenByTeacherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_AspNetUsers_FeedbackGivenByTeacherId",
                table: "Submissions");

            migrationBuilder.DropIndex(
                name: "IX_Submissions_FeedbackGivenByTeacherId",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "Feedback",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FeedbackGivenAt",
                table: "Submissions");

            migrationBuilder.DropColumn(
                name: "FeedbackGivenByTeacherId",
                table: "Submissions");
        }
    }
}
