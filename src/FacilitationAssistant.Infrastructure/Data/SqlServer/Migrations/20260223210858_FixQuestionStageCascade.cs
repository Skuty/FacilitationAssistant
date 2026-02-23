using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilitationAssistant.Infrastructure.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class FixQuestionStageCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_AgendaStages_AssociatedStageId",
                table: "Questions");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_AgendaStages_AssociatedStageId",
                table: "Questions",
                column: "AssociatedStageId",
                principalTable: "AgendaStages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Questions_AgendaStages_AssociatedStageId",
                table: "Questions");

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_AgendaStages_AssociatedStageId",
                table: "Questions",
                column: "AssociatedStageId",
                principalTable: "AgendaStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
