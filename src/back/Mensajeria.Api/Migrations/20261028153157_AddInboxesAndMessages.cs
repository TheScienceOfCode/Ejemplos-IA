using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mensajeria.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxesAndMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buzones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buzones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Buzones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mensajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RemitenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    BuzonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Asunto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Contenido = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensajes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensajes_Buzones_BuzonId",
                        column: x => x.BuzonId,
                        principalTable: "Buzones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensajes_Usuarios_RemitenteId",
                        column: x => x.RemitenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buzones_UsuarioId",
                table: "Buzones",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_BuzonId",
                table: "Mensajes",
                column: "BuzonId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensajes_RemitenteId",
                table: "Mensajes",
                column: "RemitenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mensajes");

            migrationBuilder.DropTable(
                name: "Buzones");
        }
    }
}
