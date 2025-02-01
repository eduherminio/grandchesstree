using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrandChessTree.Api.Migrations
{
    /// <inheritdoc />
    public partial class SearchItemDuplicates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "duplicates",
                table: "d10_search_items",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "duplicates",
                table: "d10_search_items");
        }
    }
}
