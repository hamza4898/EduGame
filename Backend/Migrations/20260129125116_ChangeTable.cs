using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduGameProject.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudentEducation",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "StudentGender",
                table: "Students",
                newName: "Gender");

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "Students",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Education",
                table: "Students");

            migrationBuilder.RenameColumn(
                name: "Gender",
                table: "Students",
                newName: "StudentGender");

            migrationBuilder.AddColumn<int>(
                name: "StudentEducation",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
