using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaChamados.DTOs;

public class ComentarioRespostaDto
{
    public Guid Id { get; set; }
    public Guid ChamadoId { get; set; }
    public Guid UsuarioId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
    public DateTime DataComentario { get; set; }

    public string NomeUsuario { get; set; } = string.Empty;
}