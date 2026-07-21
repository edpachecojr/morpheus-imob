using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Tentativa de revincular a outra organização uma entidade já vinculada. O
/// vínculo é imutável para que um registro não migre de tenant por engano.
/// </summary>
public sealed class ErroDeVinculoDeOrganizacaoImutavel : ErroDeRegraDeNegocio
{
    public Guid VinculoAtual { get; }
    public Guid VinculoRecusado { get; }

    public ErroDeVinculoDeOrganizacaoImutavel(Guid vinculoAtual, Guid vinculoRecusado)
        : base(ErrosDeIsolamento.VinculoImutavel(vinculoAtual, vinculoRecusado))
    {
        VinculoAtual = vinculoAtual;
        VinculoRecusado = vinculoRecusado;
    }
}
