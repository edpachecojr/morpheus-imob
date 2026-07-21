using Morpheus.Dominio.Organizacoes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

/// <summary>
/// Prova que a organização nasce utilizável (E1-F1-H2): fuso e janela de
/// atendimento preenchidos, sem passar por tela de configuração.
/// </summary>
public sealed class ConfiguracaoDaOrganizacaoTestes
{
    [Fact]
    public void Padrao_traz_fuso_brasileiro_e_horario_comercial()
    {
        var padrao = ConfiguracaoDaOrganizacao.Padrao();

        Assert.Equal("America/Sao_Paulo", padrao.FusoHorario);
        Assert.Equal(new TimeOnly(9, 0), padrao.JanelaDeAtendimento.Inicio);
        Assert.Equal(new TimeOnly(18, 0), padrao.JanelaDeAtendimento.Fim);
    }

    [Fact]
    public void Fuso_vazio_e_recusado()
    {
        var resultado = ConfiguracaoDaOrganizacao.Definir("  ", JanelaDeAtendimento.Padrao);

        Assert.Equal(ErrosDeOrganizacao.FusoHorarioObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Janela_invertida_e_recusada_citando_os_horarios()
    {
        var resultado = JanelaDeAtendimento.Definir(new TimeOnly(18, 0), new TimeOnly(9, 0));

        Assert.Equal("Organizacao.JanelaDeAtendimentoInvertida", resultado.Erro.Codigo);
        Assert.Contains("18:00", resultado.Erro.Descricao, StringComparison.Ordinal);
        Assert.Contains("09:00", resultado.Erro.Descricao, StringComparison.Ordinal);
    }

    [Fact]
    public void Janela_de_duracao_zero_e_recusada()
        => Assert.True(JanelaDeAtendimento.Definir(new TimeOnly(9, 0), new TimeOnly(9, 0)).Falha);
}
