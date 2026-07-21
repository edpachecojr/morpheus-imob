namespace Morpheus.Testes.Unitarios.Fakes;

/// <summary>
/// Relógio de teste que devolve sempre o mesmo instante. Mantém os testes de
/// auditoria repetíveis (F.I.R.S.T.): a data cravada na entidade não depende do
/// relógio real da máquina que roda a suíte.
/// </summary>
public sealed class RelogioFixo : TimeProvider
{
    private readonly DateTimeOffset _instante;

    public RelogioFixo(DateTimeOffset instante) => _instante = instante;

    public override DateTimeOffset GetUtcNow() => _instante;
}
