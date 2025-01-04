using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class BlockPropertiesCellShownAnswerId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CellShownAnswerId",
                table: "BlockProperties",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CellShownAnswerId",
                table: "BlockProperties");
        }
    }
}
