using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gnosis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEtiquetasDeTarea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EtiquetaManual",
                table: "Tareas",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaEntrega",
                table: "Tareas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EtiquetaManual",
                table: "Tareas");

            migrationBuilder.DropColumn(
                name: "FechaEntrega",
                table: "Tareas");
        }
    }
}
