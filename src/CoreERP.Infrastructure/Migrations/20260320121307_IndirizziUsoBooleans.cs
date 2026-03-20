using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IndirizziUsoBooleans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Indirizzi_AnagraficaId_Tipo",
                table: "Indirizzi");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Indirizzi");

            migrationBuilder.RenameColumn(
                name: "Principale",
                table: "Indirizzi",
                newName: "IsImpianto");

            migrationBuilder.AddColumn<bool>(
                name: "IsFatturazione",
                table: "Indirizzi",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Indirizzi_AnagraficaId_IsFatturazione_IsImpianto",
                table: "Indirizzi",
                columns: new[] { "AnagraficaId", "IsFatturazione", "IsImpianto" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Indirizzi_AnagraficaId_IsFatturazione_IsImpianto",
                table: "Indirizzi");

            migrationBuilder.DropColumn(
                name: "IsFatturazione",
                table: "Indirizzi");

            migrationBuilder.RenameColumn(
                name: "IsImpianto",
                table: "Indirizzi",
                newName: "Principale");

            migrationBuilder.AddColumn<string>(
                name: "Tipo",
                table: "Indirizzi",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Indirizzi_AnagraficaId_Tipo",
                table: "Indirizzi",
                columns: new[] { "AnagraficaId", "Tipo" });
        }
    }
}
