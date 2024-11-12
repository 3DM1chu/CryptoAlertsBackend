using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CryptoAlertsBackend.Migrations
{
    /// <inheritdoc />
    public partial class TokensFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Endpoints_EndpointId",
                table: "Tokens");

            migrationBuilder.AlterColumn<int>(
                name: "EndpointId",
                table: "Tokens",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Endpoints_EndpointId",
                table: "Tokens",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tokens_Endpoints_EndpointId",
                table: "Tokens");

            migrationBuilder.AlterColumn<int>(
                name: "EndpointId",
                table: "Tokens",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Tokens_Endpoints_EndpointId",
                table: "Tokens",
                column: "EndpointId",
                principalTable: "Endpoints",
                principalColumn: "Id");
        }
    }
}
