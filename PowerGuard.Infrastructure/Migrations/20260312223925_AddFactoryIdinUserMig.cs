using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PowerGuard.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFactoryIdinUserMig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FactoryId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_FactoryId",
                table: "AspNetUsers",
                column: "FactoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Factories_FactoryId",
                table: "AspNetUsers",
                column: "FactoryId",
                principalTable: "Factories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Factories_FactoryId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_FactoryId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FactoryId",
                table: "AspNetUsers");
        }
    }
}
