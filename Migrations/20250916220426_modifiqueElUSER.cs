using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fletesProyect.Migrations
{
    /// <inheritdoc />
    public partial class modifiqueElUSER : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "direction",
                table: "AspNetUsers",
                newName: "address");

            migrationBuilder.AddColumn<DateOnly>(
                name: "birthdate",
                table: "AspNetUsers",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "birthdate",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "AspNetUsers",
                newName: "direction");
        }
    }
}
