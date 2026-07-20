namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Unidade de isolamento de dados (o tenant): uma imobiliária ou um corretor
/// autônomo. Toda entidade de negócio pertence a exatamente uma organização —
/// a própria organização é a única exceção, por ser a raiz do isolamento.
/// Exemplo: <c>new Organizacao("Imobiliária Aurora", TimeProvider.System)</c>.
/// </summary>
public sealed class Organizacao
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public DateTimeOffset CriadaEm { get; private set; }

    public Organizacao(string nome, TimeProvider relogio)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da organização não pode ser vazio.", nameof(nome));

        Id = Guid.NewGuid();
        Nome = nome.Trim();
        CriadaEm = relogio.GetUtcNow();
    }

    // Exigido pelo materializador do EF Core; nunca usado pelo domínio.
    private Organizacao()
    {
        Nome = string.Empty;
    }
}
