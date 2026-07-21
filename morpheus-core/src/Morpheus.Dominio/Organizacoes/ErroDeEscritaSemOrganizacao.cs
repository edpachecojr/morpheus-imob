using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Escrita de entidade sem organização quando não há contexto para carimbá-la.
/// Cobre o caminho assíncrono (job/bootstrap): sem tenant explícito, a operação
/// falha em vez de gravar um dado "global" e silenciosamente órfão.
/// </summary>
public sealed class ErroDeEscritaSemOrganizacao : ErroDeRegraDeNegocio
{
    public string Entidade { get; }

    public ErroDeEscritaSemOrganizacao(string entidade)
        : base(ErrosDeIsolamento.EscritaSemOrganizacao(entidade))
    {
        Entidade = entidade;
    }
}
