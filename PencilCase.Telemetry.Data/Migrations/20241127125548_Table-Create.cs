using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Telemetry.Data.Migrations
{
    /// <inheritdoc />
    public partial class TableCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenerationResultEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PromptTokenCount = table.Column<int>(type: "integer", nullable: false),
                    GenerationTokenCount = table.Column<int>(type: "integer", nullable: false),
                    GenerationCharCount = table.Column<int>(type: "integer", nullable: false),
                    FinishReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationResultEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenerationResultEntries");
        }
    }
}
