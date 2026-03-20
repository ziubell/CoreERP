using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloAnagrafica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contatti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cognome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Cellulare = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BrevoContactId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrevoSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DataCancellazione = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contatti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetodiPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Codice = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RichiedeIBAN = table.Column<bool>(type: "bit", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodiPagamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MotiviDisattivazione",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MotiviDisattivazione", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RuoliContatto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    Ordine = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RuoliContatto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StoricoModifiche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntitaTipo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntitaId = table.Column<int>(type: "int", nullable: false),
                    Campo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ValorePrecedente = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ValoreNuovo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ValorePrecedenteLabel = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValoreNuovoLabel = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModificatoDa = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoricoModifiche", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Anagrafiche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodiceCliente = table.Column<int>(type: "int", nullable: true),
                    TipoSoggetto = table.Column<int>(type: "int", nullable: false),
                    RagioneSociale = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Cognome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Denominazione = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Attivo = table.Column<bool>(type: "bit", nullable: false),
                    MotivoDisattivazioneId = table.Column<int>(type: "int", nullable: true),
                    DataConversione = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PartitaIva = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    CodiceFiscale = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    CodiceSDI = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: true),
                    PEC = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IndirizzoFatturazione = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    CAP = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Citta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Provincia = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    Nazione = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    SitoWeb = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MetodoPagamentoId = table.Column<int>(type: "int", nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    PeriodicitaPagamento = table.Column<int>(type: "int", nullable: true),
                    BrevoCompanyId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BrevoSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DataCancellazione = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anagrafiche", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anagrafiche_MetodiPagamento_MetodoPagamentoId",
                        column: x => x.MetodoPagamentoId,
                        principalTable: "MetodiPagamento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Anagrafiche_MotiviDisattivazione_MotivoDisattivazioneId",
                        column: x => x.MotivoDisattivazioneId,
                        principalTable: "MotiviDisattivazione",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnagraficaContatti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnagraficaId = table.Column<int>(type: "int", nullable: false),
                    ContattoId = table.Column<int>(type: "int", nullable: false),
                    RuoloContattoId = table.Column<int>(type: "int", nullable: false),
                    Principale = table.Column<bool>(type: "bit", nullable: false),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnagraficaContatti", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnagraficaContatti_Anagrafiche_AnagraficaId",
                        column: x => x.AnagraficaId,
                        principalTable: "Anagrafiche",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnagraficaContatti_Contatti_ContattoId",
                        column: x => x.ContattoId,
                        principalTable: "Contatti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnagraficaContatti_RuoliContatto_RuoloContattoId",
                        column: x => x.RuoloContattoId,
                        principalTable: "RuoliContatto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnagraficaContatti_AnagraficaId_ContattoId",
                table: "AnagraficaContatti",
                columns: new[] { "AnagraficaId", "ContattoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnagraficaContatti_ContattoId",
                table: "AnagraficaContatti",
                column: "ContattoId");

            migrationBuilder.CreateIndex(
                name: "IX_AnagraficaContatti_RuoloContattoId",
                table: "AnagraficaContatti",
                column: "RuoloContattoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_CodiceCliente",
                table: "Anagrafiche",
                column: "CodiceCliente",
                unique: true,
                filter: "CodiceCliente IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_CodiceFiscale",
                table: "Anagrafiche",
                column: "CodiceFiscale",
                unique: true,
                filter: "CodiceFiscale IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_Denominazione",
                table: "Anagrafiche",
                column: "Denominazione");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_MetodoPagamentoId",
                table: "Anagrafiche",
                column: "MetodoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_MotivoDisattivazioneId",
                table: "Anagrafiche",
                column: "MotivoDisattivazioneId");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_PartitaIva",
                table: "Anagrafiche",
                column: "PartitaIva",
                unique: true,
                filter: "PartitaIva IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Anagrafiche_Tipo_Attivo",
                table: "Anagrafiche",
                columns: new[] { "Tipo", "Attivo" });

            migrationBuilder.CreateIndex(
                name: "IX_Contatti_Cellulare",
                table: "Contatti",
                column: "Cellulare",
                unique: true,
                filter: "Cellulare IS NOT NULL AND IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Contatti_Email",
                table: "Contatti",
                column: "Email",
                unique: true,
                filter: "Email IS NOT NULL AND IsDeleted = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MetodiPagamento_Codice",
                table: "MetodiPagamento",
                column: "Codice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetodiPagamento_Nome",
                table: "MetodiPagamento",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MotiviDisattivazione_Nome",
                table: "MotiviDisattivazione",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RuoliContatto_Nome",
                table: "RuoliContatto",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoricoModifiche_EntitaTipo_EntitaId_DataModifica",
                table: "StoricoModifiche",
                columns: new[] { "EntitaTipo", "EntitaId", "DataModifica" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnagraficaContatti");

            migrationBuilder.DropTable(
                name: "StoricoModifiche");

            migrationBuilder.DropTable(
                name: "Anagrafiche");

            migrationBuilder.DropTable(
                name: "Contatti");

            migrationBuilder.DropTable(
                name: "RuoliContatto");

            migrationBuilder.DropTable(
                name: "MetodiPagamento");

            migrationBuilder.DropTable(
                name: "MotiviDisattivazione");
        }
    }
}
