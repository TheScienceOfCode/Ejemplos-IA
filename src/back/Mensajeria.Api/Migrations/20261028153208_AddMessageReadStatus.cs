using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mensajeria.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageReadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EstaLeido",
                table: "Mensajes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstaLeido",
                table: "Mensajes");
        }
    }
}
