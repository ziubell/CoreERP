using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupAndBooleans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RetiRiferimento",
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
                    table.PrimaryKey("PK_RetiRiferimento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipiTecnologia",
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
                    table.PrimaryKey("PK_TipiTecnologia", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RetiRiferimento_Nome",
                table: "RetiRiferimento",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipiTecnologia_Nome",
                table: "TipiTecnologia",
                column: "Nome",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetiRiferimento");

            migrationBuilder.DropTable(
                name: "TipiTecnologia");
        }
    }
}
