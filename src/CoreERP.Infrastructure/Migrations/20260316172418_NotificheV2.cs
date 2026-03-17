using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotificheV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImpostazioniNotificaUtente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GiorniRetention = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpostazioniNotificaUtente", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SottoscrizioniNotifica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    EntitaTipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntitaId = table.Column<int>(type: "int", nullable: false),
                    DataSottoscrizione = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SottoscrizioniNotifica", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImpostazioniNotificaUtente_UserId",
                table: "ImpostazioniNotificaUtente",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SottoscrizioniNotifica_EntitaTipo_EntitaId",
                table: "SottoscrizioniNotifica",
                columns: new[] { "EntitaTipo", "EntitaId" });

            migrationBuilder.CreateIndex(
                name: "IX_SottoscrizioniNotifica_UserId_EntitaTipo_EntitaId",
                table: "SottoscrizioniNotifica",
                columns: new[] { "UserId", "EntitaTipo", "EntitaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImpostazioniNotificaUtente");

            migrationBuilder.DropTable(
                name: "SottoscrizioniNotifica");
        }
    }
}
