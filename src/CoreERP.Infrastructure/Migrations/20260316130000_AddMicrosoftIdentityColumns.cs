using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMicrosoftIdentityColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MicrosoftId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftEmail",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCollegamentoMicrosoft",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftAccessToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MicrosoftRefreshToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MicrosoftTokenExpiry",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "MicrosoftId", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MicrosoftEmail", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DataCollegamentoMicrosoft", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MicrosoftAccessToken", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MicrosoftRefreshToken", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "MicrosoftTokenExpiry", table: "AspNetUsers");
        }
    }
}
