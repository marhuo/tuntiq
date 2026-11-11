using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TuntiQ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateTable(
                name: "dimensions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    allow_multiple = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dimensions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "dimension_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dimension_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_dimension_events_dimensions_dimension_id",
                        column: x => x.dimension_id,
                        principalTable: "dimensions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dimension_names",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name_search = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(name)", stored: true, collation: "C"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dimension_names", x => x.id);
                    table.ForeignKey(
                        name: "fk_dimension_names_dimensions_dimension_id",
                        column: x => x.dimension_id,
                        principalTable: "dimensions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dimension_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reference_id = table.Column<Guid>(type: "uuid", nullable: true),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dimension_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_dimension_values_dimensions_dimension_id",
                        column: x => x.dimension_id,
                        principalTable: "dimensions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dimension_value_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension_value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dimension_value_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_dimension_value_events_dimension_values_dimension_value_id",
                        column: x => x.dimension_value_id,
                        principalTable: "dimension_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dimension_value_names",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dimension_value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name_search = table.Column<string>(type: "text", nullable: true, computedColumnSql: "lower(name)", stored: true, collation: "C"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_dimension_value_names", x => x.id);
                    table.ForeignKey(
                        name: "fk_dimension_value_names_dimension_values_dimension_value_id",
                        column: x => x.dimension_value_id,
                        principalTable: "dimension_values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_dimension_events_dimension_id",
                table: "dimension_events",
                column: "dimension_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_events_tenant_id",
                table: "dimension_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_names_dimension_id_language",
                table: "dimension_names",
                columns: new[] { "dimension_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dimension_names_tenant_id",
                table: "dimension_names",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_names_tenant_id_language_name_search",
                table: "dimension_names",
                columns: new[] { "tenant_id", "language", "name_search" });

            migrationBuilder.CreateIndex(
                name: "ix_dimension_value_events_dimension_value_id",
                table: "dimension_value_events",
                column: "dimension_value_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_value_events_tenant_id",
                table: "dimension_value_events",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_value_names_dimension_value_id_language",
                table: "dimension_value_names",
                columns: new[] { "dimension_value_id", "language" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dimension_value_names_tenant_id",
                table: "dimension_value_names",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_value_names_tenant_id_language_name_search",
                table: "dimension_value_names",
                columns: new[] { "tenant_id", "language", "name_search" });

            migrationBuilder.CreateIndex(
                name: "ix_dimension_values_dimension_id",
                table: "dimension_values",
                column: "dimension_id");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_values_tenant_id",
                table: "dimension_values",
                column: "tenant_id",
                filter: "deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_values_tenant_id_enabled",
                table: "dimension_values",
                columns: new[] { "tenant_id", "enabled" },
                filter: "deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_dimension_values_tenant_id_reference_id",
                table: "dimension_values",
                columns: new[] { "tenant_id", "reference_id" },
                unique: true,
                filter: "deleted = false AND reference_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_dimensions_tenant_id",
                table: "dimensions",
                column: "tenant_id",
                filter: "deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dimension_events");

            migrationBuilder.DropTable(
                name: "dimension_names");

            migrationBuilder.DropTable(
                name: "dimension_value_events");

            migrationBuilder.DropTable(
                name: "dimension_value_names");

            migrationBuilder.DropTable(
                name: "dimension_values");

            migrationBuilder.DropTable(
                name: "dimensions");
        }
    }
}
