namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Papel do usuário no painel, conforme a matriz de autorização (RBAC simples).
/// No MVP apenas <see cref="Dono"/> e <see cref="Corretor"/> existem; gestor e
/// secretaria entram na Fase 5 sem migração de dados.
/// </summary>
public enum PapelDoUsuario
{
    Dono = 1,
    Corretor = 2,
}
