using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class boatupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boat_crew_profiles_ProfileLoginId",
                table: "boat_crew");

            migrationBuilder.DropForeignKey(
                name: "FK_boats_profiles_ProfileLoginId",
                table: "boats");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_profiles_LoginId",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_boats_ProfileLoginId",
                table: "boats");

            migrationBuilder.DropIndex(
                name: "IX_boat_crew_ProfileLoginId",
                table: "boat_crew");

            migrationBuilder.DropIndex(
                name: "IX_boat_crew_ProfileLoginId_BoatId",
                table: "boat_crew");

            migrationBuilder.DropColumn(
                name: "ProfileLoginId",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "ProfileLoginId",
                table: "boat_crew");

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "boats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "boat_crew",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_boats_ProfileId",
                table: "boats",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_ProfileId",
                table: "boat_crew",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_ProfileId_BoatId",
                table: "boat_crew",
                columns: new[] { "ProfileId", "BoatId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_boat_crew_profiles_ProfileId",
                table: "boat_crew",
                column: "ProfileId",
                principalTable: "profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boats_profiles_ProfileId",
                table: "boats",
                column: "ProfileId",
                principalTable: "profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_boat_crew_profiles_ProfileId",
                table: "boat_crew");

            migrationBuilder.DropForeignKey(
                name: "FK_boats_profiles_ProfileId",
                table: "boats");

            migrationBuilder.DropIndex(
                name: "IX_boats_ProfileId",
                table: "boats");

            migrationBuilder.DropIndex(
                name: "IX_boat_crew_ProfileId",
                table: "boat_crew");

            migrationBuilder.DropIndex(
                name: "IX_boat_crew_ProfileId_BoatId",
                table: "boat_crew");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "boats");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "boat_crew");

            migrationBuilder.AddColumn<string>(
                name: "ProfileLoginId",
                table: "boats",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileLoginId",
                table: "boat_crew",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_profiles_LoginId",
                table: "profiles",
                column: "LoginId");

            migrationBuilder.CreateIndex(
                name: "IX_boats_ProfileLoginId",
                table: "boats",
                column: "ProfileLoginId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_ProfileLoginId",
                table: "boat_crew",
                column: "ProfileLoginId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_ProfileLoginId_BoatId",
                table: "boat_crew",
                columns: new[] { "ProfileLoginId", "BoatId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_boat_crew_profiles_ProfileLoginId",
                table: "boat_crew",
                column: "ProfileLoginId",
                principalTable: "profiles",
                principalColumn: "LoginId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_boats_profiles_ProfileLoginId",
                table: "boats",
                column: "ProfileLoginId",
                principalTable: "profiles",
                principalColumn: "LoginId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
