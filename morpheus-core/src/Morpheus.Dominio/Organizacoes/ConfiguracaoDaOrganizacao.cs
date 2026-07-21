using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Organizacoes;

/// <summary>
/// Ajustes operacionais que toda organização precisa ter preenchidos para o
/// produto funcionar: em que fuso ela vive e em que horário atende. Nasce com
/// <see cref="Padrao"/> no cadastro (E1-F1-H2) — organização recém-criada é
/// utilizável no mesmo minuto, sem passar por tela de configuração.
/// </summary>
public sealed record ConfiguracaoDaOrganizacao
{
    /// <summary>
    /// Fuso do público-alvo do MVP (imobiliárias e corretores no Brasil). Id IANA,
    /// não Windows: é o que o Postgres e o <c>TimeZoneInfo</c> em Linux entendem.
    /// </summary>
    public const string FusoHorarioPadrao = "America/Sao_Paulo";

    public string FusoHorario { get; private set; }
    public JanelaDeAtendimento JanelaDeAtendimento { get; private set; }

    private ConfiguracaoDaOrganizacao(string fusoHorario, JanelaDeAtendimento janelaDeAtendimento)
    {
        FusoHorario = fusoHorario;
        JanelaDeAtendimento = janelaDeAtendimento;
    }

    // Exigido pelo materializador do EF Core: navegação para tipo owned não pode
    // entrar por construtor. Nunca usado pelo domínio.
    private ConfiguracaoDaOrganizacao()
    {
        FusoHorario = FusoHorarioPadrao;
        JanelaDeAtendimento = JanelaDeAtendimento.Padrao;
    }

    /// <summary>Configuração utilizável de fábrica, aplicada a toda organização recém-fundada.</summary>
    public static ConfiguracaoDaOrganizacao Padrao()
        => new(FusoHorarioPadrao, JanelaDeAtendimento.Padrao);

    /// <summary>
    /// Define a configuração validando o fuso. Exemplo:
    /// <c>ConfiguracaoDaOrganizacao.Definir("America/Manaus", janela)</c>.
    /// </summary>
    public static Resultado<ConfiguracaoDaOrganizacao> Definir(
        string fusoHorario, JanelaDeAtendimento janelaDeAtendimento)
    {
        if (string.IsNullOrWhiteSpace(fusoHorario))
            return ErrosDeOrganizacao.FusoHorarioObrigatorio;
        return new ConfiguracaoDaOrganizacao(fusoHorario.Trim(), janelaDeAtendimento);
    }

    /// <summary>Reconstrói a configuração de dados já persistidos, sem revalidar.</summary>
    public static ConfiguracaoDaOrganizacao Rehidratar(string fusoHorario, JanelaDeAtendimento janelaDeAtendimento)
        => new(fusoHorario, janelaDeAtendimento);
}
