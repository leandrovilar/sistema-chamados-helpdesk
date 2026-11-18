using System.ComponentModel.DataAnnotations;
using SistemaChamados.Models;

namespace SistemaChamados.DTOs
{
    public class AtualizarChamadoDto
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres.")]
        public string Descricao { get; set; } = string.Empty;

        [Range(0, 3, ErrorMessage = "Prioridade inválida.")]
        public Prioridade Prioridade { get; set; }
    }
}