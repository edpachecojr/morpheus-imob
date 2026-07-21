using Microsoft.AspNetCore.Identity;
using Morpheus.Infraestrutura.Identidade;

namespace Morpheus.Testes.Unitarios.Identidade;

/// <summary>
/// Prova o hash de senha com Argon2id. Usa parâmetros de custo mínimo: o que se
/// verifica aqui é o comportamento (confere, recusa, sal único, pede regravação),
/// e o custo real de produção só faria a suíte demorar.
/// </summary>
public sealed class HashDeSenhaArgon2idTestes
{
    private static readonly ParametrosDeArgon2id Baratos = new(MemoriaEmKib: 1024, Iteracoes: 1, Paralelismo: 1);
    private static readonly UsuarioDaOrganizacao Qualquer = new();
    private const string Senha = "uma-senha-bem-longa";

    private readonly HashDeSenhaArgon2id _hasher = new(Baratos);

    [Fact]
    public void Confere_a_senha_correta()
    {
        var hash = _hasher.HashPassword(Qualquer, Senha);

        Assert.Equal(PasswordVerificationResult.Success, _hasher.VerifyHashedPassword(Qualquer, hash, Senha));
    }

    [Fact]
    public void Recusa_a_senha_errada()
    {
        var hash = _hasher.HashPassword(Qualquer, Senha);

        Assert.Equal(
            PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(Qualquer, hash, "outra-senha-longa"));
    }

    [Fact]
    public void Duas_gravacoes_da_mesma_senha_produzem_hashes_diferentes()
    {
        // Sal aleatório por gravação: sem isso, senhas iguais delatariam umas às
        // outras e um dicionário pré-calculado quebraria todas de uma vez.
        Assert.NotEqual(_hasher.HashPassword(Qualquer, Senha), _hasher.HashPassword(Qualquer, Senha));
    }

    [Fact]
    public void Hash_com_custo_antigo_confere_mas_pede_regravacao()
    {
        var antigo = new HashDeSenhaArgon2id(Baratos with { Iteracoes = 2 }).HashPassword(Qualquer, Senha);

        Assert.Equal(
            PasswordVerificationResult.SuccessRehashNeeded,
            _hasher.VerifyHashedPassword(Qualquer, antigo, Senha));
    }

    [Fact]
    public void Hash_corrompido_recusa_em_vez_de_lancar()
        => Assert.Equal(
            PasswordVerificationResult.Failed,
            _hasher.VerifyHashedPassword(Qualquer, "lixo-no-banco", Senha));

    [Fact]
    public void Hash_gravado_nao_contem_a_senha_em_claro()
    {
        var hash = _hasher.HashPassword(Qualquer, Senha);

        Assert.DoesNotContain(Senha, hash, StringComparison.Ordinal);
    }
}
