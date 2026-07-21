using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Usuarios;

/// <summary>
/// Completa "os meus dados" do onboarding (E1-F1-H4): o usuário autenticado
/// substitui o próprio nome completo. Não exige permissão especial — qualquer
/// papel edita o próprio nome, diferente de renomear a organização inteira.
/// </summary>
public sealed class AtualizacaoDeDadosDoUsuario
{
    private readonly IContextoDoUsuario _usuarioAtual;
    private readonly IAtualizacaoDeUsuario _atualizacao;

    public AtualizacaoDeDadosDoUsuario(IContextoDoUsuario usuarioAtual, IAtualizacaoDeUsuario atualizacao)
    {
        _usuarioAtual = usuarioAtual;
        _atualizacao = atualizacao;
    }

    /// <summary>
    /// Atualiza o nome completo do usuário da sessão corrente.
    /// Exemplo: <c>await atualizacao.ExecutarAsync("Ana Souza", cancelamento)</c>.
    /// </summary>
    public Task<Resultado> ExecutarAsync(string nomeCompleto, CancellationToken cancelamento)
    {
        var usuarioId = _usuarioAtual.UsuarioAutenticadoId ?? throw new ErroDeUsuarioNaoAutenticado();
        return _atualizacao.AtualizarNomeCompletoAsync(usuarioId, nomeCompleto, cancelamento);
    }
}
