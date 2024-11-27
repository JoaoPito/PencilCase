using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Telemetry.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgentsAddedModelColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "GenerationResultEntries",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Model",
                table: "GenerationResultEntries");
        }
    }
}
