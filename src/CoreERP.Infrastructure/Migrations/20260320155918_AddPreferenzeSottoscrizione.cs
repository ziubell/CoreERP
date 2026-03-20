using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreferenzeSottoscrizione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificaContatti",
                table: "SottoscrizioniNotifica",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificaIndirizzi",
                table: "SottoscrizioniNotifica",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificaMessaggi",
                table: "SottoscrizioniNotifica",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotificaModifiche",
                table: "SottoscrizioniNotifica",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificaContatti",
                table: "SottoscrizioniNotifica");

            migrationBuilder.DropColumn(
                name: "NotificaIndirizzi",
                table: "SottoscrizioniNotifica");

            migrationBuilder.DropColumn(
                name: "NotificaMessaggi",
                table: "SottoscrizioniNotifica");

            migrationBuilder.DropColumn(
                name: "NotificaModifiche",
                table: "SottoscrizioniNotifica");
        }
    }
}
