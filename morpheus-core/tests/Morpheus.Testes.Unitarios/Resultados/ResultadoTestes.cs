using Morpheus.Dominio.Resultados;

namespace Morpheus.Testes.Unitarios.Resultados;

public sealed class ResultadoTestes
{
    private static readonly Erro ErroDeExemplo = new("Teste.Falha", "Falha de exemplo.");

    [Fact]
    public void Sucesso_sem_valor_nao_carrega_erro()
    {
        var resultado = Resultado.DeSucesso();

        Assert.True(resultado.Sucesso);
        Assert.False(resultado.Falha);
        Assert.Equal(Erro.Nenhum, resultado.Erro);
    }

    [Fact]
    public void Falha_carrega_o_erro_informado()
    {
        var resultado = Resultado.DeFalha(ErroDeExemplo);

        Assert.True(resultado.Falha);
        Assert.Equal(ErroDeExemplo, resultado.Erro);
    }

    [Fact]
    public void Sucesso_com_valor_expoe_o_valor()
    {
        var resultado = Resultado.DeSucesso(42);

        Assert.True(resultado.Sucesso);
        Assert.Equal(42, resultado.Valor);
    }

    [Fact]
    public void Acessar_valor_de_falha_falha_alto()
    {
        var resultado = Resultado.DeFalha<int>(ErroDeExemplo);

        Assert.Throws<InvalidOperationException>(() => resultado.Valor);
    }

    [Fact]
    public void Valor_converte_implicitamente_em_sucesso()
    {
        Resultado<int> resultado = 7;

        Assert.True(resultado.Sucesso);
        Assert.Equal(7, resultado.Valor);
    }

    [Fact]
    public void Erro_converte_implicitamente_em_falha()
    {
        Resultado<int> resultado = ErroDeExemplo;

        Assert.True(resultado.Falha);
        Assert.Equal(ErroDeExemplo, resultado.Erro);
    }

    [Fact]
    public void Falha_sem_erro_e_estado_impossivel()
    {
        Assert.Throws<InvalidOperationException>(() => Resultado.DeFalha(Erro.Nenhum));
    }
}
