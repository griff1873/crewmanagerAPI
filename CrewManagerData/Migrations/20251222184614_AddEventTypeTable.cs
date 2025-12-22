using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrewManagerData.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create event_type table
            migrationBuilder.Sql(@"
                CREATE TABLE event_type (
                    ""Id"" SERIAL PRIMARY KEY,
                    ""ProfileId"" INTEGER NULL,
                    ""Name"" VARCHAR(200) NOT NULL,
                    ""CreatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""UpdatedAt"" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
                    ""IsDeleted"" BOOLEAN NOT NULL DEFAULT FALSE,
                    ""DeletedBy"" TEXT NULL,
                    ""DeletedAt"" TIMESTAMP WITH TIME ZONE NULL,
                    ""CreatedBy"" TEXT NULL,
                    ""UpdatedBy"" TEXT NULL
                );
            ");

            // Create indexes
            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_event_type_Name"" ON event_type (""Name"");
                CREATE INDEX ""IX_event_type_ProfileId"" ON event_type (""ProfileId"");
            ");

            // Insert default event types
            migrationBuilder.Sql(@"
                INSERT INTO event_type (""Name"", ""ProfileId"", ""CreatedAt"", ""UpdatedAt"", ""IsDeleted"") 
                VALUES 
                    ('Race', NULL, NOW(), NOW(), FALSE),
                    ('Cruise', NULL, NOW(), NOW(), FALSE),
                    ('Training', NULL, NOW(), NOW(), FALSE);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the event_type table
            migrationBuilder.Sql("DROP TABLE IF EXISTS event_type;");
        }
    }
}
