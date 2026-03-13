using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilitationAssistant.Infrastructure.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageAuthorName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Messages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "Messages");
        }
    }
}
