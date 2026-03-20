using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMessaggiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messaggi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntitaTipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntitaId = table.Column<int>(type: "int", nullable: false),
                    Testo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messaggi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllegatiMessaggio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessaggioId = table.Column<int>(type: "int", nullable: false),
                    NomeFile = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Percorso = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Dimensione = table.Column<long>(type: "bigint", nullable: false),
                    DataCaricamento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllegatiMessaggio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllegatiMessaggio_Messaggi_MessaggioId",
                        column: x => x.MessaggioId,
                        principalTable: "Messaggi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllegatiMessaggio_MessaggioId",
                table: "AllegatiMessaggio",
                column: "MessaggioId");

            migrationBuilder.CreateIndex(
                name: "IX_Messaggi_EntitaTipo_EntitaId_DataCreazione",
                table: "Messaggi",
                columns: new[] { "EntitaTipo", "EntitaId", "DataCreazione" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllegatiMessaggio");

            migrationBuilder.DropTable(
                name: "Messaggi");
        }
    }
}
