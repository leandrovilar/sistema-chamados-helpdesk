using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SistemaChamados.Models;

namespace SistemaChamados.DTOs;

public class AlterarStatusDto
{
    public StatusChamado Status { get; set; }
}