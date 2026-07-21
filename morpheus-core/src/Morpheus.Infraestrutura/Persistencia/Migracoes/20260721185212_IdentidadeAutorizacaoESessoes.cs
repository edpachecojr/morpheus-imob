using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Morpheus.Infraestrutura.Persistencia.Migracoes
{
    /// <inheritdoc />
    public partial class IdentidadeAutorizacaoESessoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "papel",
                table: "usuarios");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "atendimento_fim",
                table: "organizacoes",
                type: "time without time zone",
                nullable: false,
                // Mesmos padrões de ConfiguracaoDaOrganizacao.Padrao(): organização
                // anterior a esta migração fica utilizável, não com janela zerada.
                defaultValue: new TimeOnly(18, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "atendimento_inicio",
                table: "organizacoes",
                type: "time without time zone",
                nullable: false,
                defaultValue: new TimeOnly(9, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "fuso_horario",
                table: "organizacoes",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "America/Sao_Paulo");

            migrationBuilder.CreateTable(
                name: "sessoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    conteudo = table.Column<byte[]>(type: "bytea", nullable: false),
                    expira_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    criada_em = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessoes", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessoes_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "concurrency_stamp", "name", "normalized_name" },
                values: new object[,]
                {
                    { new Guid("9f1b7a10-0000-4000-8000-000000000001"), "9f1b7a10-0000-4000-8000-000000000001", "dono", "DONO" },
                    { new Guid("9f1b7a10-0000-4000-8000-000000000002"), "9f1b7a10-0000-4000-8000-000000000002", "corretor", "CORRETOR" }
                });

            migrationBuilder.InsertData(
                table: "role_claims",
                columns: new[] { "id", "claim_type", "claim_value", "role_id" },
                values: new object[,]
                {
                    { 1, "permissao", "imovel.ler", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 2, "permissao", "imovel.escrever", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 3, "permissao", "lead.ler", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 4, "permissao", "lead.atender", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 5, "permissao", "visita.agendar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 6, "permissao", "dossie.criar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 7, "permissao", "dossie.ler", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 8, "permissao", "dossie.aprovar_item", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 9, "permissao", "dossie.baixar_anexo", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 10, "permissao", "magiclink.emitir", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 11, "permissao", "magiclink.revogar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 12, "permissao", "relatorio.gerar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 13, "permissao", "relatorio.enviar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 14, "permissao", "usuario.gerenciar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 15, "permissao", "tenant.configurar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 16, "permissao", "faturamento.gerenciar", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 17, "permissao", "metricas.ler", new Guid("9f1b7a10-0000-4000-8000-000000000001") },
                    { 18, "permissao", "imovel.ler", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 19, "permissao", "imovel.escrever", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 20, "permissao", "lead.ler", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 21, "permissao", "lead.atender", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 22, "permissao", "visita.agendar", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 23, "permissao", "dossie.criar", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 24, "permissao", "dossie.ler", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 25, "permissao", "dossie.aprovar_item", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 26, "permissao", "dossie.baixar_anexo", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 27, "permissao", "magiclink.emitir", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 28, "permissao", "magiclink.revogar", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 29, "permissao", "relatorio.gerar", new Guid("9f1b7a10-0000-4000-8000-000000000002") },
                    { 30, "permissao", "relatorio.enviar", new Guid("9f1b7a10-0000-4000-8000-000000000002") }
                });

            migrationBuilder.CreateIndex(
                name: "ix_sessoes_usuario_id",
                table: "sessoes",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sessoes");

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "role_claims",
                keyColumn: "id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("9f1b7a10-0000-4000-8000-000000000001"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("9f1b7a10-0000-4000-8000-000000000002"));

            migrationBuilder.DropColumn(
                name: "atendimento_fim",
                table: "organizacoes");

            migrationBuilder.DropColumn(
                name: "atendimento_inicio",
                table: "organizacoes");

            migrationBuilder.DropColumn(
                name: "fuso_horario",
                table: "organizacoes");

            migrationBuilder.AddColumn<string>(
                name: "papel",
                table: "usuarios",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
