using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrandChessTree.Api.Migrations
{
    /// <inheritdoc />
    public partial class Tasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "d10_search_task");

            migrationBuilder.CreateTable(
                name: "d10_search_items",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    available_at = table.Column<long>(type: "bigint", nullable: false),
                    pass_count = table.Column<long>(type: "bigint", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_d10_search_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "d10_search_tasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    search_item_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    started_at = table.Column<long>(type: "bigint", nullable: false),
                    finished_at = table.Column<long>(type: "bigint", nullable: false),
                    nps = table.Column<float>(type: "real", nullable: false),
                    nodes = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    captures = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    enpassants = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    castles = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    promotions = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    direct_checks = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    single_discovered_check = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    direct_discovered_check = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    double_discovered_check = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    direct_checkmate = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    single_discovered_checkmate = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    direct_discoverd_checkmate = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    double_discoverd_checkmate = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_d10_search_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_d10_search_tasks_d10_search_items_search_item_id",
                        column: x => x.search_item_id,
                        principalTable: "d10_search_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_d10_search_tasks_search_item_id",
                table: "d10_search_tasks",
                column: "search_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "d10_search_tasks");

            migrationBuilder.DropTable(
                name: "d10_search_items");

            migrationBuilder.CreateTable(
                name: "d10_search_task",
                columns: table => new
                {
                    id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    available_at = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    pass_count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_d10_search_task", x => x.id);
                });
        }
    }
}
