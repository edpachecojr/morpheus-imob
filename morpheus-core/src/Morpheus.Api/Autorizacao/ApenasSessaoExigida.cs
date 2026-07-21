namespace Morpheus.Api.Autorizacao;

/// <summary>
/// Rota que exige sessão válida e nada além dela. É o caso de operações sobre a
/// própria sessão — encerrar a sua — em que exigir permissão nomeada seria
/// inventar uma permissão que nenhum papel poderia negar sem prender o usuário
/// dentro do sistema.
/// </summary>
public sealed record ApenasSessaoExigida : IDeclaracaoDeAcesso;
