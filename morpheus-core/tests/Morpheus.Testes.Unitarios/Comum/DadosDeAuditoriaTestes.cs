using Morpheus.Dominio.Comum;
using Morpheus.Testes.Unitarios.Fakes;

namespace Morpheus.Testes.Unitarios.Comum;

public sealed class DadosDeAuditoriaTestes
{
    private static readonly DateTimeOffset Nascimento = new(2026, 7, 21, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Depois = new(2026, 7, 22, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Nascer_iguala_criado_e_atualizado_ao_instante_do_relogio()
    {
        var auditoria = DadosDeAuditoria.Nascer(new RelogioFixo(Nascimento));

        Assert.Equal(Nascimento, auditoria.CriadoEm);
        Assert.Equal(Nascimento, auditoria.AtualizadoEm);
    }

    [Fact]
    public void RegistrarAlteracao_avanca_atualizado_e_preserva_criado()
    {
        var original = DadosDeAuditoria.Nascer(new RelogioFixo(Nascimento));

        var alterada = original.RegistrarAlteracao(new RelogioFixo(Depois));

        Assert.Equal(Nascimento, alterada.CriadoEm);
        Assert.Equal(Depois, alterada.AtualizadoEm);
    }

    [Fact]
    public void RegistrarAlteracao_nao_muta_a_instancia_original()
    {
        var original = DadosDeAuditoria.Nascer(new RelogioFixo(Nascimento));

        original.RegistrarAlteracao(new RelogioFixo(Depois));

        Assert.Equal(Nascimento, original.AtualizadoEm);
    }

    [Fact]
    public void Rehidratar_preserva_os_dois_instantes_sem_relogio()
    {
        var auditoria = DadosDeAuditoria.Rehidratar(Nascimento, Depois);

        Assert.Equal(Nascimento, auditoria.CriadoEm);
        Assert.Equal(Depois, auditoria.AtualizadoEm);
    }
}
