using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class boatImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "boats",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "boats");
        }
    }
}
