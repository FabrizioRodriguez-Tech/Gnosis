using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gnosis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaCompletadaYDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCompletada",
                table: "Tareas",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCompletada",
                table: "Tareas");
        }
    }
}
