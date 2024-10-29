using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableParentsBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "Block",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block",
                column: "ParentId",
                principalTable: "Block",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "Block",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Block_Block_ParentId",
                table: "Block",
                column: "ParentId",
                principalTable: "Block",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
