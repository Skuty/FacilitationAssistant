using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilitationAssistant.Infrastructure.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorNameAndAnonymousAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowAnonymousAnswers",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "QuestionResponses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Notes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowAnonymousAnswers",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "QuestionResponses");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "Notes");
        }
    }
}
