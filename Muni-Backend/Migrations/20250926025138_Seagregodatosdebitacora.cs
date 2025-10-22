using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fletesProyect.Migrations
{
    /// <inheritdoc />
    public partial class Seagregodatosdebitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "userCreateId",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "userCreateId",
                table: "AspNetRoles",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_userCreateId",
                table: "AspNetUsers",
                column: "userCreateId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_userCreateId",
                table: "AspNetRoles",
                column: "userCreateId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_AspNetUsers_userCreateId",
                table: "AspNetRoles",
                column: "userCreateId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_userCreateId",
                table: "AspNetUsers",
                column: "userCreateId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_AspNetUsers_userCreateId",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_userCreateId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_userCreateId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_userCreateId",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "userCreateId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "userCreateId",
                table: "AspNetRoles");
        }
    }
}
