using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SistemaChamados.Models
{
    public class Chamado
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "O título é obrigatório.")]
        [StringLength(150, ErrorMessage = "O título deve ter no máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adescrição é obrigatória")]
        [StringLength(2000, ErrorMessage = "A descrição deve ter no máximo 2000 caracteres." )]
    public string Descricao { get; set; } = string.Empty;
        public StatusChamado Status { get; set; }

        [Range(0, 3, ErrorMessage = "Prioridade inválida.")]
        public Prioridade Prioridade { get; set; }

        // Relacionamento
        [Required(ErrorMessage = "O solicitante é obrigatório.")]
        public Guid SolicitanteId { get; set; }
        public Usuario? Solicitante { get; set; }

        public Guid? AtendenteId { get; set; }
        public Usuario? Atendente { get; set; }

        // Datas
        public DateTime DataAbertura { get; set; }
        public DateTime? DataFechamento { get; set; }

        //Coleção de comentários (relacionamento 1:N)
        public List<Comentario>? Comentarios { get; set; }
    }
}