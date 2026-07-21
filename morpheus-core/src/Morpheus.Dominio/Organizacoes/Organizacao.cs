using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Unidade de isolamento de dados (o tenant): uma imobiliária ou um corretor
/// autônomo. Toda entidade de negócio pertence a exatamente uma organização —
/// a própria organização é a única exceção, por ser a raiz do isolamento.
/// Nasce por <see cref="Fundar"/>; volta do banco por <see cref="Rehidratar"/>.
/// </summary>
public sealed class Organizacao
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }

    private Organizacao(Guid id, string nome, DateTimeOffset criadaEm)
    {
        Id = id;
        Nome = nome;
        CriadaEm = criadaEm;
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Organizacao()
    {
        Nome = string.Empty;
    }

    /// <summary>
    /// Funda uma organização nova: gera identidade e auditoria e valida o nome.
    /// Nome vazio é desfecho esperado, então vira <see cref="Resultado"/>.
    /// Exemplo: <c>Organizacao.Fundar("Imobiliária Aurora", relogio)</c>.
    /// </summary>
    public static Resultado<Organizacao> Fundar(string nome, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(nome))
            return ErrosDeOrganizacao.NomeObrigatorio;

        return new Organizacao(Guid.NewGuid(), nome.Trim(), relogio.GetUtcNow());
    }

    /// <summary>
    /// Reconstrói uma organização já persistida a partir de dados confiáveis: não
    /// gera identidade nem auditoria e não revalida. Exemplo:
    /// <c>Organizacao.Rehidratar(id, "Imobiliária Aurora", quando)</c>.
    /// </summary>
    public static Organizacao Rehidratar(Guid id, string nome, DateTimeOffset criadaEm)
        => new(id, nome, criadaEm);
}
