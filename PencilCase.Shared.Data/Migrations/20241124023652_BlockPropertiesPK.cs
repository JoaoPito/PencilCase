using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class BlockPropertiesPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BlockProperties",
                table: "BlockProperties");

            migrationBuilder.DropIndex(
                name: "IX_BlockProperties_ParentId",
                table: "BlockProperties");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BlockProperties");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlockProperties",
                table: "BlockProperties",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BlockProperties",
                table: "BlockProperties");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "BlockProperties",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlockProperties",
                table: "BlockProperties",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BlockProperties_ParentId",
                table: "BlockProperties",
                column: "ParentId",
                unique: true);
        }
    }
}
