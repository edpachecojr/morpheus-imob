using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Morpheus.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class TituloFinalidadeESituacaoDoImovel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "finalidade",
                table: "imoveis",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Locacao");

            migrationBuilder.AddColumn<string>(
                name: "situacao",
                table: "imoveis",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Disponivel");

            migrationBuilder.AddColumn<string>(
                name: "titulo",
                table: "imoveis",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "finalidade",
                table: "imoveis");

            migrationBuilder.DropColumn(
                name: "situacao",
                table: "imoveis");

            migrationBuilder.DropColumn(
                name: "titulo",
                table: "imoveis");
        }
    }
}
