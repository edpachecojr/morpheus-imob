using Morpheus.Dominio.Erros;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Operação de isolamento acionada com um OrganizacaoId vazio. Sinaliza contexto
/// de tenant quebrado antes que uma escrita órfã ou leitura sem filtro aconteça —
/// carrega o nome da operação para dizer onde o vínculo faltou.
/// </summary>
public sealed class ErroDeOrganizacaoObrigatoria : ErroDeRegraDeNegocio
{
    public string Operacao { get; }

    public ErroDeOrganizacaoObrigatoria(string operacao)
        : base(ErrosDeIsolamento.OrganizacaoObrigatoria(operacao))
    {
        Operacao = operacao;
    }
}
