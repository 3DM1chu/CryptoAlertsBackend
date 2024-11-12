using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoAlertsBackend.Migrations
{
    /// <inheritdoc />
    public partial class AssetSymbolToName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Symbol",
                table: "Assets",
                newName: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Assets",
                newName: "Symbol");
        }
    }
}
