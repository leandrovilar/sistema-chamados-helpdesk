using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaChamados.DTOs;

public class CriarComentarioDto
{
    public Guid ChamadoId { get; set; }
    public string Conteudo { get; set; } = string.Empty;
}