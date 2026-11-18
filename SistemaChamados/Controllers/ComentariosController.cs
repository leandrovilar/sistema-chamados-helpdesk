using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaChamados.Data;
using SistemaChamados.DTOs;
using SistemaChamados.Models;

namespace SistemaChamados.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly ChamadosContext _context;

        public ComentariosController(ChamadosContext context)
        {
            _context = context;
        }

        private bool TentarObterUsuarioId(out Guid usuarioId)
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            return Guid.TryParse(valor, out usuarioId);
        }

        // GET: api/Comentarios
        [HttpGet]
        [Authorize(Roles = "Solicitante,Atendente,Admin")]
        public async Task<IActionResult> GetComentarios()
        {
            var comentarios = await _context.Comentarios
                .Select(c => new ComentarioRespostaDto
                {
                    Id = c.Id,
                    ChamadoId = c.ChamadoId,
                    UsuarioId = c.UsuarioId,
                    Conteudo = c.Conteudo,
                    DataComentario = c.DataComentario,
                    NomeUsuario = c.Usuario == null
                        ? string.Empty
                        : c.Usuario.Nome
                })
                .ToListAsync();

            return Ok(comentarios);
        }

        // GET: api/Comentarios/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Solicitante,Atendente,Admin")]
        public async Task<IActionResult> GetComentario(Guid id)
        {
            var comentario = await _context.Comentarios
                .Where(c => c.Id == id)
                .Select(c => new ComentarioRespostaDto
                {
                    Id = c.Id,
                    ChamadoId = c.ChamadoId,
                    UsuarioId = c.UsuarioId,
                    Conteudo = c.Conteudo,
                    DataComentario = c.DataComentario,
                    NomeUsuario = c.Usuario == null
                        ? string.Empty
                        : c.Usuario.Nome
                })
                .FirstOrDefaultAsync();

            if (comentario == null)
            {
                return NotFound();
            }

            return Ok(comentario);
        }

        // GET: api/Comentarios/chamados/{chamadoId}
        [HttpGet("chamados/{chamadoId}")]
        [Authorize(Roles = "Solicitante,Atendente,Admin")]
        public async Task<IActionResult> GetComentariosPorChamado(
            Guid chamadoId)
        {
            var comentarios = await _context.Comentarios
                .Where(c => c.ChamadoId == chamadoId)
                .OrderBy(c => c.DataComentario)
                .Select(c => new ComentarioRespostaDto
                {
                    Id = c.Id,
                    ChamadoId = c.ChamadoId,
                    UsuarioId = c.UsuarioId,
                    Conteudo = c.Conteudo,
                    DataComentario = c.DataComentario,
                    NomeUsuario = c.Usuario == null
                        ? string.Empty
                        : c.Usuario.Nome
                })
                .ToListAsync();

            return Ok(comentarios);
        }

        // POST: api/Comentarios
        [HttpPost]
        [Authorize(Roles = "Solicitante,Atendente,Admin")]
        public async Task<IActionResult> PostComentario(
            [FromBody] CriarComentarioDto dto)
        {
            if (!TentarObterUsuarioId(out var usuarioId))
            {
                return Unauthorized();
            }

            var chamadoExiste = await _context.Chamados
                .AnyAsync(c => c.Id == dto.ChamadoId);

            if (!chamadoExiste)
            {
                return BadRequest("Chamado inválido.");
            }

            var comentario = new Comentario
            {
                ChamadoId = dto.ChamadoId,
                UsuarioId = usuarioId,
                Conteudo = dto.Conteudo,
                DataComentario = DateTime.UtcNow
            };

            _context.Comentarios.Add(comentario);
            await _context.SaveChangesAsync();

            var resposta = await _context.Comentarios
                .Where(c => c.Id == comentario.Id)
                .Select(c => new ComentarioRespostaDto
                {
                    Id = c.Id,
                    ChamadoId = c.ChamadoId,
                    UsuarioId = c.UsuarioId,
                    Conteudo = c.Conteudo,
                    DataComentario = c.DataComentario,
                    NomeUsuario = c.Usuario == null
                        ? string.Empty
                        : c.Usuario.Nome
                })
                .FirstAsync();

            return CreatedAtAction(
                nameof(GetComentario),
                new { id = comentario.Id },
                resposta);
        }

        // PUT: api/Comentarios/{id}
        // Somente o autor ou Admin pode editar.
        [HttpPut("{id}")]
        [Authorize(Roles = "Solicitante,Atendente,Admin")]
        public async Task<IActionResult> PutComentario(
            Guid id,
            [FromBody] AtualizarComentarioDto dto)
        {
            if (!TentarObterUsuarioId(out var usuarioId))
            {
                return Unauthorized();
            }

            var comentario = await _context.Comentarios.FindAsync(id);

            if (comentario == null)
            {
                return NotFound();
            }

            var ehAutor = comentario.UsuarioId == usuarioId;
            var ehAdmin = User.IsInRole("Admin");

            if (!ehAutor && !ehAdmin)
            {
                return Forbid();
            }

            comentario.Conteudo = dto.Conteudo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Comentarios/{id}
        // Somente Admin pode excluir.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteComentario(Guid id)
        {
            var comentario = await _context.Comentarios.FindAsync(id);

            if (comentario == null)
            {
                return NotFound();
            }

            _context.Comentarios.Remove(comentario);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}