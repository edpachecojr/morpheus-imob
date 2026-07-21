using Morpheus.Dominio.Erros;
using Morpheus.Dominio.Resultados;

namespace Morpheus.Dominio.Usuarios;

/// <summary>
/// Definição do nome completo do usuário com valor vazio. Enquanto o cadastro de
/// conta (E1-F1-H2) não expõe uma factory com <see cref="Resultado"/>, a mutação
/// isolada rejeita entrada vazia como violação de regra, não como ArgumentException.
/// </summary>
public sealed class ErroDeNomeCompletoObrigatorio : ErroDeRegraDeNegocio
{
    public ErroDeNomeCompletoObrigatorio() : base(ErrosDeUsuario.NomeCompletoObrigatorio)
    {
    }
}
