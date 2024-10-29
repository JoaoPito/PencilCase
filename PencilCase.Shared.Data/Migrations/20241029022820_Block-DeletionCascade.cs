using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class BlockDeletionCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block");

            migrationBuilder.AddForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block",
                column: "ParentId",
                principalTable: "Block",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block");

            migrationBuilder.AddForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block",
                column: "ParentId",
                principalTable: "Block",
                principalColumn: "Id");
        }
    }
}
