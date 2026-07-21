using Morpheus.Aplicacao.Comum;
using Morpheus.Aplicacao.Organizacoes;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Resultados;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Aplicacao.Contas;

/// <summary>
/// Cria conta e tenant de uma vez (E1-F1-H2): funda a organização, cria o usuário
/// dono e os vincula — tudo numa transação só, para não existir organização sem
/// dono nem dono sem organização.
/// <para>
/// A organização nasce com o nome do fundador e a configuração padrão; renomeá-la
/// e completar o cadastro é assunto do onboarding, que não bloqueia o primeiro uso.
/// </para>
/// <para>
/// <b>E-mail já cadastrado devolve sucesso</b>, igual ao cadastro real. É
/// deliberado: qualquer resposta diferente transforma o formulário público num
/// verificador de "esta pessoa é cliente?". Quem já tem conta recebe o aviso pelo
/// e-mail de onboarding, não pela resposta HTTP.
/// </para>
/// </summary>
public sealed class CadastroDeConta
{
    private readonly IExecucaoTransacional _transacao;
    private readonly IRepositorioDeOrganizacoes _organizacoes;
    private readonly IRegistroDeUsuarios _usuarios;
    private readonly TimeProvider _relogio;

    public CadastroDeConta(
        IExecucaoTransacional transacao,
        IRepositorioDeOrganizacoes organizacoes,
        IRegistroDeUsuarios usuarios,
        TimeProvider relogio)
    {
        _transacao = transacao;
        _organizacoes = organizacoes;
        _usuarios = usuarios;
        _relogio = relogio;
    }

    /// <summary>
    /// Executa o cadastro. Devolve sucesso tanto para conta criada quanto para
    /// e-mail já existente — quem chama responde igual nos dois casos.
    /// Exemplo: <c>await cadastro.ExecutarAsync(dados, cancelamento)</c>.
    /// </summary>
    public async Task<Resultado> ExecutarAsync(DadosDoCadastro dados, CancellationToken cancelamento)
    {
        var entrada = dados.Validar();
        if (entrada.Falha)
            return entrada;

        // Antes da checagem de existência: senha fraca recusada só no caminho de
        // e-mail novo seria oráculo de enumeração.
        var senha = await _usuarios.ValidarSenhaAsync(dados.Senha, cancelamento);
        if (senha.Falha)
            return senha;

        if (await _usuarios.ExisteComEmailAsync(dados.Email, cancelamento))
            return Resultado.DeSucesso();

        var cadastro = await _transacao.ExecutarAsync(
            interno => FundarOrganizacaoComDonoAsync(dados, interno), cancelamento);

        // Corrida perdida pelo mesmo e-mail: a transação já foi desfeita e a
        // resposta continua indistinguível da de um cadastro bem-sucedido.
        if (cadastro.Falha && cadastro.Erro == ErrosDeCadastro.EmailJaCadastrado)
            return Resultado.DeSucesso();

        return cadastro;
    }

    private async Task<Resultado> FundarOrganizacaoComDonoAsync(
        DadosDoCadastro dados, CancellationToken cancelamento)
    {
        var fundacao = Organizacao.Fundar(dados.NomeCompleto, _relogio);
        if (fundacao.Falha)
            return fundacao;

        await _organizacoes.AdicionarAsync(fundacao.Valor, cancelamento);

        var novo = new NovoUsuarioDoPainel(
            fundacao.Valor.Id, dados.NomeCompleto, dados.Email, dados.Senha, PapeisDoUsuario.Dono);
        var criacao = await _usuarios.CriarAsync(novo, cancelamento);

        return criacao.Falha ? Resultado.DeFalha(criacao.Erro) : Resultado.DeSucesso();
    }
}
