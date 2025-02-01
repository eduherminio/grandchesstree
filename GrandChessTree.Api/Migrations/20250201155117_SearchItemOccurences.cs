using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrandChessTree.Api.Migrations
{
    /// <inheritdoc />
    public partial class SearchItemOccurences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "duplicates",
                table: "d10_search_items",
                newName: "occurrences");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "occurrences",
                table: "d10_search_items",
                newName: "duplicates");
        }
    }
}
