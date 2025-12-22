using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeIdToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add EventTypeId column to events table
            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to first event type (Race)

            // Create index for EventTypeId
            migrationBuilder.CreateIndex(
                name: "IX_events_EventTypeId",
                table: "events",
                column: "EventTypeId");

            // Add foreign key constraint
            migrationBuilder.AddForeignKey(
                name: "FK_events_event_type_EventTypeId",
                table: "events",
                column: "EventTypeId",
                principalTable: "event_type",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_events_event_type_EventTypeId",
                table: "events");

            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_events_EventTypeId",
                table: "events");

            // Drop EventTypeId column
            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "events");
        }
    }
}
