using Microsoft.AspNetCore.Identity;
using Morpheus.Dominio.Comum;
using Morpheus.Dominio.Organizacoes;
using Morpheus.Dominio.Usuarios;

namespace Morpheus.Infraestrutura.Identidade;

/// <summary>
/// Usuário do painel persistido pelo IdentityCore, vinculado a uma organização.
/// Mora na infraestrutura porque carrega o acoplamento ao SDK do Identity — o
/// domínio conhece apenas <see cref="IPertenceOrganizacao"/>.
/// <para>
/// <b>Não guarda papel.</b> Quem guarda o vínculo usuário → papel é o próprio
/// Identity, nas tabelas <c>roles</c> e <c>user_roles</c> (ADR-0010): um enum aqui
/// seria uma segunda verdade sobre a mesma coisa — e a que ninguém consulta na
/// hora de autorizar.
/// </para>
/// <para>
/// Implementa <see cref="IPossuiEventosDeDominio"/> para que o cadastro entre no
/// outbox pela mesma via de qualquer escrita, embora o SDK exija construção sem
/// parâmetros e impeça a herança de <c>EntidadeBase</c>.
/// </para>
/// </summary>
public sealed class UsuarioDaOrganizacao : IdentityUser<Guid>, IPertenceOrganizacao, IPossuiEventosDeDominio
{
    private readonly List<IEventoDeDominio> _eventosDeDominio = [];

    public Guid OrganizacaoId { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;

    public IReadOnlyCollection<IEventoDeDominio> EventosDeDominio => _eventosDeDominio;

    /// <summary>
    /// Cria o usuário do painel já vinculado à organização e registra
    /// <see cref="UsuarioCadastradoEvento"/> para o outbox drenar na mesma transação
    /// do cadastro. O papel entra como dado do evento; quem o efetiva no Identity é
    /// quem persiste. Exemplo:
    /// <c>UsuarioDaOrganizacao.Cadastrar(new OrganizacaoDona(orgId), "Ana Souza", "ana@exemplo.com", PapeisDoUsuario.Dono, relogio)</c>.
    /// </summary>
    public static UsuarioDaOrganizacao Cadastrar(
        OrganizacaoDona organizacao,
        string nomeCompleto,
        string email,
        string papel,
        TimeProvider relogio)
    {
        var usuario = new UsuarioDaOrganizacao { Id = Guid.NewGuid(), UserName = email, Email = email };
        usuario.VincularAOrganizacao(organizacao);
        usuario.DefinirNomeCompleto(nomeCompleto);
        usuario._eventosDeDominio.Add(new UsuarioCadastradoEvento(
            usuario.Id, usuario.NomeCompleto, email, papel, relogio.GetUtcNow()));
        return usuario;
    }

    /// <summary>
    /// Vincula o usuário à sua organização (não vazia, garantida pelo VO). Imutável:
    /// tentar revincular a outra organização é recusado, para o usuário não migrar de
    /// tenant por engano. Exemplo: <c>usuario.VincularAOrganizacao(new OrganizacaoDona(orgId))</c>.
    /// </summary>
    public void VincularAOrganizacao(OrganizacaoDona organizacao)
    {
        if (OrganizacaoId != Guid.Empty && OrganizacaoId != organizacao.Valor)
            throw new ErroDeVinculoDeOrganizacaoImutavel(OrganizacaoId, organizacao.Valor);
        OrganizacaoId = organizacao.Valor;
    }

    public void DefinirNomeCompleto(string nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ErroDeNomeCompletoObrigatorio();
        NomeCompleto = nomeCompleto.Trim();
    }

    public void LimparEventos() => _eventosDeDominio.Clear();
}
