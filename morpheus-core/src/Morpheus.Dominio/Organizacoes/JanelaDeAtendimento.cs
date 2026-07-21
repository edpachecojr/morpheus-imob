using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Faixa de horário em que a organização atende — o intervalo dentro do qual a
/// triagem oferece visita e o dunning dispara cobrança. Value object: a invariante
/// "fim depois do início" mora aqui, não espalhada em quem agenda.
/// </summary>
public sealed record JanelaDeAtendimento
{
    /// <summary>Comercial brasileiro, das 9h às 18h: o que serve a organização recém-criada sem configurar nada.</summary>
    public static readonly JanelaDeAtendimento Padrao = new(new TimeOnly(9, 0), new TimeOnly(18, 0));

    public TimeOnly Inicio { get; private set; }
    public TimeOnly Fim { get; private set; }

    private JanelaDeAtendimento(TimeOnly inicio, TimeOnly fim)
    {
        Inicio = inicio;
        Fim = fim;
    }

    // Exigido pelo materializador do EF Core: tipo owned aninhado não é construído
    // por parâmetro. Nunca usado pelo domínio, que passa sempre por Definir/Rehidratar.
    private JanelaDeAtendimento()
    {
    }

    /// <summary>
    /// Cria a janela validando a ordem dos horários. Exemplo:
    /// <c>JanelaDeAtendimento.Definir(new TimeOnly(8, 0), new TimeOnly(19, 0))</c>.
    /// </summary>
    public static Resultado<JanelaDeAtendimento> Definir(TimeOnly inicio, TimeOnly fim)
    {
        if (fim <= inicio)
            return ErrosDeOrganizacao.JanelaDeAtendimentoInvertida(inicio, fim);
        return new JanelaDeAtendimento(inicio, fim);
    }

    /// <summary>
    /// Reconstrói a janela de dados já persistidos, sem revalidar — o banco é a
    /// fonte de verdade e o EF materializa por aqui.
    /// </summary>
    public static JanelaDeAtendimento Rehidratar(TimeOnly inicio, TimeOnly fim) => new(inicio, fim);
}
