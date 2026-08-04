using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gnosis.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSiembraEnfoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiembrasEnfoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Crecio = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiembrasEnfoque", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiembrasEnfoque_UsuarioId",
                table: "SiembrasEnfoque",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiembrasEnfoque");
        }
    }
}
