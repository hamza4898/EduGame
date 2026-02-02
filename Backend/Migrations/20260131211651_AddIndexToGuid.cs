using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduGameProject.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Teachers_ExternalId",
                table: "Teachers",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ExternalId",
                table: "Students",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partners_ExternalId",
                table: "Partners",
                column: "ExternalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teachers_ExternalId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Students_ExternalId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Partners_ExternalId",
                table: "Partners");
        }
    }
}
