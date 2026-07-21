using Morpheus.Dominio.Comum;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Unidade de isolamento de dados (o tenant): uma imobiliária ou um corretor
/// autônomo. Toda entidade de negócio pertence a exatamente uma organização —
/// a própria organização é a única exceção, por ser a raiz do isolamento, e por
/// isso herda <see cref="EntidadeBase"/> mas não o vínculo de tenant.
/// Nasce por <see cref="Fundar"/>; volta do banco por <see cref="Rehidratar"/>.
/// </summary>
public sealed class Organizacao : EntidadeBase
{
    public string Nome { get; private set; }

    /// <summary>Instante da fundação — a linguagem da organização para <see cref="DadosDeAuditoria.CriadoEm"/>.</summary>
    public DateTimeOffset CriadaEm => Auditoria.CriadoEm;

    private Organizacao(Guid id, string nome, DadosDeAuditoria auditoria)
    {
        Id = id;
        Nome = nome;
        Auditoria = auditoria;
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Organizacao()
    {
        Nome = string.Empty;
    }

    /// <summary>
    /// Funda uma organização nova: gera identidade e auditoria, valida o nome e
    /// registra <see cref="OrganizacaoFundadaEvento"/> para o outbox drenar na escrita.
    /// Nome vazio é desfecho esperado, então vira <see cref="Resultado"/>.
    /// Exemplo: <c>Organizacao.Fundar("Imobiliária Aurora", relogio)</c>.
    /// </summary>
    public static Resultado<Organizacao> Fundar(string nome, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return ErrosDeOrganizacao.NomeObrigatorio;

        var organizacao = new Organizacao(Guid.NewGuid(), nome.Trim(), DadosDeAuditoria.Nascer(relogio));
        organizacao.RegistrarEvento(new OrganizacaoFundadaEvento(
            organizacao.Id, organizacao.Nome, organizacao.CriadaEm));
        return organizacao;
    }

    /// <summary>
    /// Reconstrói uma organização já persistida a partir de dados confiáveis: não
    /// gera identidade nem auditoria, não revalida e não registra evento.
    /// Exemplo: <c>Organizacao.Rehidratar(id, "Imobiliária Aurora", criadoEm, atualizadoEm)</c>.
    /// </summary>
    public static Organizacao Rehidratar(Guid id, string nome, DateTimeOffset criadoEm, DateTimeOffset atualizadoEm)
        => new(id, nome, DadosDeAuditoria.Rehidratar(criadoEm, atualizadoEm));
}
