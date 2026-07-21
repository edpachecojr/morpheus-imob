namespace Morpheus.Infraestrutura.Identidade;

/// <summary>Partes de um hash Argon2id lido do banco.</summary>
public sealed record HashArgon2idDecodificado(ParametrosDeArgon2id Parametros, byte[] Sal, byte[] Resumo);
