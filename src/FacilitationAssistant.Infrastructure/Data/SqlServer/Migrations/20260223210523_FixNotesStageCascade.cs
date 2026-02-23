using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacilitationAssistant.Infrastructure.Data.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class FixNotesStageCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AgendaStages_StageId",
                table: "Notes");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AgendaStages_StageId",
                table: "Notes",
                column: "StageId",
                principalTable: "AgendaStages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_AgendaStages_StageId",
                table: "Notes");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_AgendaStages_StageId",
                table: "Notes",
                column: "StageId",
                principalTable: "AgendaStages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
