using Morpheus.Aplicacao.Imoveis;
using Morpheus.Dominio.Imoveis;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Api.Endpoints.Imoveis;

/// <summary>
/// Entrada crua da listagem de imóveis, como chega da query string (E2-F1-H2).
/// Finalidade e situação ficam como texto aqui porque o bind automático de enum
/// da Minimal API não devolve erro com contexto — <see cref="ParaFiltro"/> converte
/// citando o valor recebido e os valores aceitos.
/// Exemplo: <c>parametros.ParaFiltro()</c>.
/// </summary>
public sealed class ParametrosDeListagemDeImoveis
{
    public string? Busca { get; init; }
    public string? Finalidade { get; init; }
    public string? Situacao { get; init; }

    // Nulável de propósito: o bind de query string da Minimal API não respeita
    // inicializador de propriedade em ausência do parâmetro — zeraria em vez de
    // cair no padrão. O padrão vem explícito em ParaFiltro.
    public int? Pagina { get; init; }
    public int? TamanhoDaPagina { get; init; }

    public Resultado<FiltroDeListagemDeImoveis> ParaFiltro()
    {
        if (!TentarConverter<FinalidadeDoImovel>(Finalidade, out var finalidade))
            return ErrosDeListagemDeImoveis.FinalidadeInvalida(Finalidade!);
        if (!TentarConverter<SituacaoDoImovel>(Situacao, out var situacao))
            return ErrosDeListagemDeImoveis.SituacaoInvalida(Situacao!);

        return new FiltroDeListagemDeImoveis(
            Busca, finalidade, situacao,
            Pagina ?? 1,
            TamanhoDaPagina ?? FiltroDeListagemDeImoveis.TamanhoPadraoDaPagina);
    }

    private static bool TentarConverter<TEnum>(string? valor, out TEnum? convertido)
        where TEnum : struct, Enum
    {
        convertido = null;
        if (string.IsNullOrWhiteSpace(valor))
            return true;
        if (!Enum.TryParse<TEnum>(valor, ignoreCase: true, out var resultado))
            return false;

        convertido = resultado;
        return true;
    }
}
