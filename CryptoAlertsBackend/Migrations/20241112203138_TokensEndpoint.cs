using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoAlertsBackend.Migrations
{
    /// <inheritdoc />
    public partial class TokensEndpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EndpointId",
                table: "Tokens",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tokens_EndpointId",
                table: "Tokens",
                column: "EndpointId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Endpoints_EndpointId",
                table: "Tokens",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Endpoints_EndpointId",
                table: "Tokens");

            migrationBuilder.DropIndex(
                name: "IX_Tokens_EndpointId",
                table: "Tokens");

            migrationBuilder.DropColumn(
                name: "EndpointId",
                table: "Tokens");
        }
    }
}
