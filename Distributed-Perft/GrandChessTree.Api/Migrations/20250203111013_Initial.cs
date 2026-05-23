using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GrandChessTree.Api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "perft_items",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hash = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    available_at = table.Column<long>(type: "bigint", nullable: false),
                    pass_count = table.Column<int>(type: "integer", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    occurrences = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perft_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "api_keys",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<long>(type: "bigint", nullable: false),
                    apikey_tail = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_api_keys", x => x.id);
                    table.ForeignKey(
                        name: "FK_api_keys_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "perft_tasks",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    perft_item_id = table.Column<long>(type: "bigint", nullable: false),
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
                    double_discoverd_checkmate = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    account_id = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perft_tasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_perft_tasks_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_perft_tasks_perft_items_perft_item_id",
                        column: x => x.perft_item_id,
                        principalTable: "perft_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_api_keys_account_id",
                table: "api_keys",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_perft_items_depth",
                table: "perft_items",
                column: "depth");

            migrationBuilder.CreateIndex(
                name: "IX_perft_items_hash_depth",
                table: "perft_items",
                columns: new[] { "hash", "depth" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_perft_tasks_account_id",
                table: "perft_tasks",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_perft_tasks_depth",
                table: "perft_tasks",
                column: "depth");

            migrationBuilder.CreateIndex(
                name: "IX_perft_tasks_perft_item_id",
                table: "perft_tasks",
                column: "perft_item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "api_keys");

            migrationBuilder.DropTable(
                name: "perft_tasks");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "perft_items");
        }
    }
}
