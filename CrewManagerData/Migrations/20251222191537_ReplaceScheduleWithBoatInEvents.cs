using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceScheduleWithBoatInEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint for ScheduleId
            migrationBuilder.DropForeignKey(
                name: "FK_events_schedules_ScheduleId",
                table: "events");

            // Drop the index for ScheduleId
            migrationBuilder.DropIndex(
                name: "IX_events_ScheduleId",
                table: "events");

            // Add BoatId column
            migrationBuilder.AddColumn<int>(
                name: "BoatId",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to first boat

            // Create index for BoatId
            migrationBuilder.CreateIndex(
                name: "IX_events_BoatId",
                table: "events",
                column: "BoatId");

            // Add foreign key constraint for BoatId
            migrationBuilder.AddForeignKey(
                name: "FK_events_boats_BoatId",
                table: "events",
                column: "BoatId",
                principalTable: "boats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Drop the ScheduleId column
            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the foreign key constraint for BoatId
            migrationBuilder.DropForeignKey(
                name: "FK_events_boats_BoatId",
                table: "events");

            // Drop the index for BoatId
            migrationBuilder.DropIndex(
                name: "IX_events_BoatId",
                table: "events");

            // Add ScheduleId column back
            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Create index for ScheduleId
            migrationBuilder.CreateIndex(
                name: "IX_events_ScheduleId",
                table: "events",
                column: "ScheduleId");

            // Add foreign key constraint for ScheduleId
            migrationBuilder.AddForeignKey(
                name: "FK_events_schedules_ScheduleId",
                table: "events",
                column: "ScheduleId",
                principalTable: "schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Drop the BoatId column
            migrationBuilder.DropColumn(
                name: "BoatId",
                table: "events");
        }
    }
}
