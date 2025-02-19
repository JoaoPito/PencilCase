using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PencilCase.Identity.Migrations
{
    /// <inheritdoc />
    public partial class Usersv1rootBlock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RootBlockId",
                table: "AspNetUsers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RootBlockId",
                table: "AspNetUsers");
        }
    }
}
