using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Organizacoes;

/// <summary>
/// Completa o onboarding renomeando a organização do contexto (E1-F1-H4). Não
/// bloqueia nada: o tenant já é utilizável desde a fundação com o nome do
/// fundador — isto só substitui o placeholder pelo nome definitivo.
/// </summary>
public sealed class RenomeacaoDaOrganizacao
{
    private readonly IContextoDaOrganizacaoAtual _organizacaoAtual;
    private readonly IContextoDoUsuario _usuarioAtual;
    private readonly IRepositorioDeOrganizacoes _organizacoes;
    private readonly TimeProvider _relogio;

    public RenomeacaoDaOrganizacao(
        IContextoDaOrganizacaoAtual organizacaoAtual,
        IContextoDoUsuario usuarioAtual,
        IRepositorioDeOrganizacoes organizacoes,
        TimeProvider relogio)
    {
        _organizacaoAtual = organizacaoAtual;
        _usuarioAtual = usuarioAtual;
        _organizacoes = organizacoes;
        _relogio = relogio;
    }

    /// <summary>
    /// Renomeia a organização do usuário autenticado.
    /// Exemplo: <c>await renomeacao.ExecutarAsync("Imobiliária Aurora Ltda", cancelamento)</c>.
    /// </summary>
    public async Task<Resultado> ExecutarAsync(string novoNome, CancellationToken cancelamento)
    {
        var organizacaoId = await _organizacaoAtual.ObterOrganizacaoIdAsync(cancelamento);
        var autorId = _usuarioAtual.UsuarioAutenticadoId ?? throw new ErroDeUsuarioNaoAutenticado();

        var organizacao = await _organizacoes.ObterPorIdAsync(organizacaoId, cancelamento)
            ?? throw new InvalidOperationException(
                $"Organização {organizacaoId} do contexto não existe mais no registro.");

        var renomeacao = organizacao.Renomear(novoNome, autorId, _relogio);
        if (renomeacao.Falha)
            return renomeacao;

        await _organizacoes.AtualizarAsync(organizacao, cancelamento);
        return Resultado.DeSucesso();
    }
}
