using Morpheus.Infraestrutura.Identidade;

namespace Morpheus.Testes.Unitarios.Identidade;

/// <summary>
/// Prova o formato PHC gravado no banco. Testar a codificação em separado é o que
/// permite cobrir os casos de hash corrompido sem pagar 19 MiB de derivação por
/// caso de teste.
/// </summary>
public sealed class CodificacaoDeHashArgon2idTestes
{
    private static readonly ParametrosDeArgon2id Parametros = new(MemoriaEmKib: 19456, Iteracoes: 2, Paralelismo: 1);
    private static readonly byte[] Sal = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
    private static readonly byte[] Resumo = [9, 8, 7, 6, 5, 4, 3, 2, 1];

    [Fact]
    public void Codifica_no_formato_phc_reconhecivel()
    {
        var codificado = CodificacaoDeHashArgon2id.Codificar(Parametros, Sal, Resumo);

        Assert.StartsWith("$argon2id$v=19$m=19456,t=2,p=1$", codificado, StringComparison.Ordinal);
    }

    [Fact]
    public void Decodifica_de_volta_os_mesmos_parametros_sal_e_resumo()
    {
        var codificado = CodificacaoDeHashArgon2id.Codificar(Parametros, Sal, Resumo);

        Assert.True(CodificacaoDeHashArgon2id.TentarDecodificar(codificado, out var decodificado));
        Assert.Equal(Parametros, decodificado.Parametros);
        Assert.Equal(Sal, decodificado.Sal);
        Assert.Equal(Resumo, decodificado.Resumo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-hash")]
    [InlineData("$argon2i$v=19$m=19456,t=2,p=1$AAAA$BBBB")]
    [InlineData("$argon2id$v=19$m=19456,t=2$AAAA$BBBB")]
    [InlineData("$argon2id$v=19$m=xis,t=2,p=1$AAAA$BBBB")]
    [InlineData("$argon2id$v=19$m=19456,t=2,p=1$nao-base64!$BBBB")]
    public void Hash_fora_do_formato_e_recusado_sem_lancar(string codificado)
        => Assert.False(CodificacaoDeHashArgon2id.TentarDecodificar(codificado, out _));
}
