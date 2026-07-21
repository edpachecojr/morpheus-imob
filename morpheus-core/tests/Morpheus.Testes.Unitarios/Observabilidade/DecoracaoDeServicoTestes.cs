using Microsoft.Extensions.DependencyInjection;
using Morpheus.Infraestrutura.Observabilidade;

namespace Morpheus.Testes.Unitarios.Observabilidade;

/// <summary>
/// Prova o seam de OCP: <c>Decorar</c> troca o registro por um decorador que
/// envolve a implementação original, preservando o tempo de vida, sem que o
/// consumidor ou o serviço original saibam.
/// </summary>
public sealed class DecoracaoDeServicoTestes
{
    public interface ISaudacao
    {
        string Falar();
    }

    private sealed class SaudacaoSimples : ISaudacao
    {
        public string Falar() => "oi";
    }

    private sealed class SaudacaoEmMaiuscula : ISaudacao
    {
        private readonly ISaudacao _interno;

        public SaudacaoEmMaiuscula(ISaudacao interno) => _interno = interno;

        public string Falar() => _interno.Falar().ToUpperInvariant();
    }

    [Fact]
    public void Decorar_envolve_a_implementacao_original()
    {
        var servicos = new ServiceCollection();
        servicos.AddScoped<ISaudacao, SaudacaoSimples>();

        servicos.Decorar<ISaudacao, SaudacaoEmMaiuscula>();
        var resolvido = servicos.BuildServiceProvider().GetRequiredService<ISaudacao>();

        Assert.IsType<SaudacaoEmMaiuscula>(resolvido);
        Assert.Equal("OI", resolvido.Falar());
    }

    [Fact]
    public void Decorar_preserva_o_tempo_de_vida_original()
    {
        var servicos = new ServiceCollection();
        servicos.AddScoped<ISaudacao, SaudacaoSimples>();

        servicos.Decorar<ISaudacao, SaudacaoEmMaiuscula>();

        var registro = servicos.Single(descritor => descritor.ServiceType == typeof(ISaudacao));
        Assert.Equal(ServiceLifetime.Scoped, registro.Lifetime);
    }

    [Fact]
    public void Decorar_sem_registro_previo_falha_com_contexto()
    {
        var servicos = new ServiceCollection();

        var erro = Assert.Throws<InvalidOperationException>(
            () => servicos.Decorar<ISaudacao, SaudacaoEmMaiuscula>());

        Assert.Contains(nameof(ISaudacao), erro.Message);
    }
}
