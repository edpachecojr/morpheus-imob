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
        : base($"Vínculo de organização é imutável: entidade já pertence a {vinculoAtual} " +
               $"e não pode ser revinculada a {vinculoRecusado}.")
    {
        VinculoAtual = vinculoAtual;
        VinculoRecusado = vinculoRecusado;
    }
}
