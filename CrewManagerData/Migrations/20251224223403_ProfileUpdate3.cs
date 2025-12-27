using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class ProfileUpdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "profiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "profiles",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Zip",
                table: "profiles",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "State",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "Zip",
                table: "profiles");
        }
    }
}
