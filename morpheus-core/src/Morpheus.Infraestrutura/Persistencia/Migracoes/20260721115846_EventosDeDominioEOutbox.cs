using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Morpheus.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class EventosDeDominioEOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "criada_em",
                table: "organizacoes",
                newName: "criado_em");

            migrationBuilder.RenameColumn(
                name: "cadastrado_em",
                table: "imoveis",
                newName: "criado_em");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "atualizado_em",
                table: "organizacoes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "atualizado_em",
                table: "imoveis",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "mensagens_outbox",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organizacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_do_evento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    conteudo = table.Column<string>(type: "jsonb", nullable: false),
                    ocorrido_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processado_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_mensagens_outbox", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_mensagens_outbox_organizacao_id",
                table: "mensagens_outbox",
                column: "organizacao_id");

            migrationBuilder.CreateIndex(
                name: "ix_mensagens_outbox_processado_em_ocorrido_em",
                table: "mensagens_outbox",
                columns: new[] { "processado_em", "ocorrido_em" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensagens_outbox");

            migrationBuilder.DropColumn(
                name: "atualizado_em",
                table: "organizacoes");

            migrationBuilder.DropColumn(
                name: "atualizado_em",
                table: "imoveis");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "organizacoes",
                newName: "criada_em");

            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "imoveis",
                newName: "cadastrado_em");
        }
    }
}
