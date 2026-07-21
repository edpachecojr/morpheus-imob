using Morpheus.Aplicacao.Usuarios;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Sessoes;

/// <summary>
/// Login por e-mail e senha (E1-F2-H1). Orquestra a sequência que torna os três
/// modos de recusa — e-mail inexistente, senha errada e conta bloqueada —
/// indistinguíveis para quem está de fora: <b>todos</b> gastam o tempo de uma
/// conferência de senha e devolvem falha; quem chama responde a mesma coisa.
/// <para>
/// Os códigos de erro internos diferem de propósito, para o log distinguir ataque
/// de esquecimento. Quem responde ao cliente traduz qualquer um deles em
/// <see cref="ErrosDeAutenticacao.CredenciaisInvalidas"/>.
/// </para>
/// </summary>
public sealed class AutenticacaoDeUsuario
{
    private readonly IDiretorioDeUsuarios _usuarios;
    private readonly IVerificadorDeSenha _senhas;
    private readonly ISessaoDoPainel _sessao;

    public AutenticacaoDeUsuario(
        IDiretorioDeUsuarios usuarios, IVerificadorDeSenha senhas, ISessaoDoPainel sessao)
    {
        _usuarios = usuarios;
        _senhas = senhas;
        _sessao = sessao;
    }

    /// <summary>
    /// Confere as credenciais e, dando certo, abre a sessão do painel.
    /// Exemplo: <c>await autenticacao.ExecutarAsync("ana@exemplo.com", senha, cancelamento)</c>.
    /// </summary>
    public async Task<Resultado> ExecutarAsync(string email, string senha, CancellationToken cancelamento)
    {
        var usuario = await _usuarios.BuscarPorEmailAsync(email, cancelamento);
        if (usuario is null || usuario.Bloqueado)
            return await RecusarGastandoOMesmoTempoAsync(usuario, cancelamento);

        if (!await _senhas.ConferirAsync(usuario.Id, senha, cancelamento))
            return await RegistrarSenhaIncorretaAsync(usuario.Id, cancelamento);

        await _senhas.LimparFalhasAsync(usuario.Id, cancelamento);
        await _sessao.AbrirAsync(usuario.Id, cancelamento);
        return Resultado.DeSucesso();
    }

    // Sem conta ou com conta bloqueada não há senha a conferir — e é exatamente
    // aí que a resposta rápida denunciaria o estado da conta.
    private async Task<Resultado> RecusarGastandoOMesmoTempoAsync(
        UsuarioDoPainel? usuario, CancellationToken cancelamento)
    {
        await _senhas.ConsumirTempoEquivalenteAsync(cancelamento);
        return Resultado.DeFalha(usuario is null
            ? ErrosDeAutenticacao.ContaInexistente
            : ErrosDeAutenticacao.ContaBloqueada);
    }

    private async Task<Resultado> RegistrarSenhaIncorretaAsync(Guid usuarioId, CancellationToken cancelamento)
    {
        await _senhas.RegistrarFalhaAsync(usuarioId, cancelamento);
        return Resultado.DeFalha(ErrosDeAutenticacao.SenhaIncorreta);
    }
}
