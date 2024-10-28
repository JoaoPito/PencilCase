using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Block_Block_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Block",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockProperties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockProperties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockProperties_Block_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Block",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Block_ParentId",
                table: "Block",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockProperties_ParentId",
                table: "BlockProperties",
                column: "ParentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlockProperties");

            migrationBuilder.DropTable(
                name: "Block");
        }
    }
}
