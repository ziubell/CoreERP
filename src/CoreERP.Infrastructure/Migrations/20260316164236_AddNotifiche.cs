using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifiche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TipiNotifica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descrizione = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Icona = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Colore = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Attivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipiNotifica", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifiche",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    MittenteUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TipoNotificaId = table.Column<int>(type: "int", nullable: false),
                    Titolo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Messaggio = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Letta = table.Column<bool>(type: "bit", nullable: false),
                    DataLettura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCreazione = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatoDA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataModifica = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificatoDa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifiche", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifiche_TipiNotifica_TipoNotificaId",
                        column: x => x.TipoNotificaId,
                        principalTable: "TipiNotifica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferenzeNotificaUtente",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TipoNotificaId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<bool>(type: "bit", nullable: false),
                    Browser = table.Column<bool>(type: "bit", nullable: false),
                    Teams = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenzeNotificaUtente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreferenzeNotificaUtente_TipiNotifica_TipoNotificaId",
                        column: x => x.TipoNotificaId,
                        principalTable: "TipiNotifica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifiche_DataCreazione",
                table: "Notifiche",
                column: "DataCreazione");

            migrationBuilder.CreateIndex(
                name: "IX_Notifiche_TipoNotificaId",
                table: "Notifiche",
                column: "TipoNotificaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifiche_UserId_Letta",
                table: "Notifiche",
                columns: new[] { "UserId", "Letta" });

            migrationBuilder.CreateIndex(
                name: "IX_PreferenzeNotificaUtente_TipoNotificaId",
                table: "PreferenzeNotificaUtente",
                column: "TipoNotificaId");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenzeNotificaUtente_UserId_TipoNotificaId",
                table: "PreferenzeNotificaUtente",
                columns: new[] { "UserId", "TipoNotificaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipiNotifica_Codice",
                table: "TipiNotifica",
                column: "Codice",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifiche");

            migrationBuilder.DropTable(
                name: "PreferenzeNotificaUtente");

            migrationBuilder.DropTable(
                name: "TipiNotifica");
        }
    }
}
