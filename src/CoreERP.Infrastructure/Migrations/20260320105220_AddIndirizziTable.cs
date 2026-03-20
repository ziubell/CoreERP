using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndirizziTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Indirizzi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnagraficaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SottoTipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Rete = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Strada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Frazione = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Citta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Provincia = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    Regione = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CAP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Latitudine = table.Column<double>(type: "float", nullable: true),
                    Longitudine = table.Column<double>(type: "float", nullable: true),
                    EgonCivico = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EgonStrada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EgonLocalita = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Principale = table.Column<bool>(type: "bit", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indirizzi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indirizzi_Anagrafiche_AnagraficaId",
                        column: x => x.AnagraficaId,
                        principalTable: "Anagrafiche",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Indirizzi_AnagraficaId_Tipo",
                table: "Indirizzi",
                columns: new[] { "AnagraficaId", "Tipo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Indirizzi");
        }
    }
}
