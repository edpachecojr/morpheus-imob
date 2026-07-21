using Morpheus.Dominio.Comum;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Fato de que um usuário do painel nasceu. É o gatilho do onboarding: o e-mail de
/// boas-vindas e a cobrança dos dados que o cadastro não pediu saem daqui, e por
/// isso o evento carrega nome, e-mail e papel — não só o id.
/// <para>
/// Vai ao outbox na mesma transação que cria a organização e o usuário (E1-F1-H2),
/// então nenhum onboarding fica órfão de um cadastro que falhou no meio.
/// </para>
/// </summary>
public sealed record UsuarioCadastradoEvento(
    Guid UsuarioId,
    string NomeCompleto,
    string Email,
    string Papel,
    DateTimeOffset OcorridoEm) : IEventoDeDominio;
