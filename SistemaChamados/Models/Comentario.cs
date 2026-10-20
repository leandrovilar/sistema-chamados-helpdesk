using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SistemaChamados.Models
{
    public class Comentario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O chamado é obrigatório.")]
        public Guid ChamadoId { get; set; }
        public Chamado? Chamado { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório.")]
        public Guid UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "O conteúdo do comentário é obrigatório.")]
        [StringLength(1000, ErrorMessage = "O comentário deve ter no máximo 1000 caracteres.")]
        public string Conteudo { get; set; } = string.Empty;
        public DateTime DataComentario { get; set; }
    }
}