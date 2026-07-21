using Morpheus.Dominio.Organizacoes;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Organizacoes;

public sealed class OrganizacaoTestes
{
    private static readonly DateTimeOffset Instante = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Fundar_gera_identidade_e_auditoria_do_relogio()
    {
        var resultado = Organizacao.Fundar("Imobiliária Aurora", new RelogioFixo(Instante));

        Assert.True(resultado.Sucesso);
        Assert.NotEqual(Guid.Empty, resultado.Valor.Id);
        Assert.Equal(Instante, resultado.Valor.CriadaEm);
    }

    [Fact]
    public void Fundar_apara_espacos_do_nome()
    {
        var resultado = Organizacao.Fundar("  Imobiliária Aurora  ", new RelogioFixo(Instante));

        Assert.Equal("Imobiliária Aurora", resultado.Valor.Nome);
    }

    [Fact]
    public void Fundar_sem_nome_falha_com_nome_obrigatorio()
    {
        var resultado = Organizacao.Fundar("   ", new RelogioFixo(Instante));

        Assert.True(resultado.Falha);
        Assert.Equal(ErrosDeOrganizacao.NomeObrigatorio, resultado.Erro);
    }

    [Fact]
    public void Rehidratar_preserva_os_atributos_sem_gerar_nova_identidade()
    {
        var id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var organizacao = Organizacao.Rehidratar(id, "Imobiliária Aurora", Instante);

        Assert.Equal(id, organizacao.Id);
        Assert.Equal("Imobiliária Aurora", organizacao.Nome);
        Assert.Equal(Instante, organizacao.CriadaEm);
    }
}
