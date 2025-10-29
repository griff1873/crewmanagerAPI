using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class ScheduleBoatCrew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ScheduleId",
                table: "events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_profiles_LoginId",
                table: "profiles",
                column: "LoginId");

            migrationBuilder.CreateTable(
                name: "boats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ProfileLoginId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_boats_profiles_ProfileLoginId",
                        column: x => x.ProfileLoginId,
                        principalTable: "profiles",
                        principalColumn: "LoginId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "boat_crew",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileLoginId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BoatId = table.Column<int>(type: "integer", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boat_crew", x => x.Id);
                    table.ForeignKey(
                        name: "FK_boat_crew_boats_BoatId",
                        column: x => x.BoatId,
                        principalTable: "boats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_boat_crew_profiles_ProfileLoginId",
                        column: x => x.ProfileLoginId,
                        principalTable: "profiles",
                        principalColumn: "LoginId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BoatId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schedules_boats_BoatId",
                        column: x => x.BoatId,
                        principalTable: "boats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_profiles_LoginId",
                table: "profiles",
                column: "LoginId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_events_ScheduleId",
                table: "events",
                column: "ScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_BoatId",
                table: "boat_crew",
                column: "BoatId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_ProfileLoginId",
                table: "boat_crew",
                column: "ProfileLoginId");

            migrationBuilder.CreateIndex(
                name: "IX_boat_crew_ProfileLoginId_BoatId",
                table: "boat_crew",
                columns: new[] { "ProfileLoginId", "BoatId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_boats_Name",
                table: "boats",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_boats_ProfileLoginId",
                table: "boats",
                column: "ProfileLoginId");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_BoatId",
                table: "schedules",
                column: "BoatId");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_Name",
                table: "schedules",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_events_schedules_ScheduleId",
                table: "events",
                column: "ScheduleId",
                principalTable: "schedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_events_schedules_ScheduleId",
                table: "events");

            migrationBuilder.DropTable(
                name: "boat_crew");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropTable(
                name: "boats");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_profiles_LoginId",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_profiles_LoginId",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_events_ScheduleId",
                table: "events");

            migrationBuilder.DropColumn(
                name: "ScheduleId",
                table: "events");
        }
    }
}
