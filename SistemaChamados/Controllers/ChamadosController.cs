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
using SistemaChamados.Rules;

namespace SistemaChamados.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChamadosController : ControllerBase
    {
        private readonly ChamadosContext _context;

        public ChamadosController(ChamadosContext context)
        {
            _context = context;
        }

        private bool TentarObterUsuarioId(out Guid usuarioId)
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            return Guid.TryParse(valor, out usuarioId);
        }

        // GET: api/Chamados
        // Listagem geral para Atendente e Admin.
        [HttpGet]
        [Authorize(Roles = "Atendente,Admin")]
        public async Task<IActionResult> GetChamados()
        {
            var chamados = await _context.Chamados
                .Select(c => new
                {
                    c.Id,
                    c.Titulo,
                    c.Descricao,
                    c.Status,
                    c.Prioridade,
                    c.SolicitanteId,
                    c.AtendenteId,
                    c.DataAbertura,
                    c.DataFechamento,

                    Solicitante = c.Solicitante == null
                        ? null
                        : new
                        {
                            c.Solicitante.Id,
                            c.Solicitante.Nome
                        },

                    Atendente = c.Atendente == null
                        ? null
                        : new
                        {
                            c.Atendente.Id,
                            c.Atendente.Nome
                        }
                })
                .ToListAsync();

            return Ok(chamados);
        }

        // GET: api/Chamados/{id}
        // Solicitante consulta somente o próprio chamado.
        [HttpGet("{id}")]
        [Authorize(Roles = "Solicitante,Atendente,Admin")]
        public async Task<IActionResult> GetChamado(Guid id)
        {
            var chamado = await _context.Chamados
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Titulo,
                    c.Descricao,
                    c.Status,
                    c.Prioridade,
                    c.SolicitanteId,
                    c.AtendenteId,
                    c.DataAbertura,
                    c.DataFechamento,

                    Solicitante = c.Solicitante == null
                        ? null
                        : new
                        {
                            c.Solicitante.Id,
                            c.Solicitante.Nome
                        },

                    Atendente = c.Atendente == null
                        ? null
                        : new
                        {
                            c.Atendente.Id,
                            c.Atendente.Nome
                        }
                })
                .FirstOrDefaultAsync();

            if (chamado == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin") &&
                !User.IsInRole("Atendente"))
            {
                if (!TentarObterUsuarioId(out var usuarioId))
                {
                    return Unauthorized();
                }

                if (chamado.SolicitanteId != usuarioId)
                {
                    return Forbid();
                }
            }

            return Ok(chamado);
        }

        // POST: api/Chamados
        // Solicitante cria em seu nome; Admin informa o solicitante.
        [HttpPost]
        [Authorize(Roles = "Solicitante,Admin")]
        public async Task<IActionResult> PostChamado(
            [FromBody] CriarChamadoDto dto)
        {
            var solicitanteId = dto.SolicitanteId;

            if (!User.IsInRole("Admin"))
            {
                if (!TentarObterUsuarioId(out var usuarioId))
                {
                    return Unauthorized();
                }

                solicitanteId = usuarioId;
            }

            if (solicitanteId == Guid.Empty)
            {
                return BadRequest("O solicitante é obrigatório.");
            }

            var solicitante = await _context.Usuarios
                .FindAsync(solicitanteId);

            if (solicitante == null ||
                !solicitante.Ativo ||
                solicitante.Papel != Papel.Solicitante)
            {
                return BadRequest("Solicitante inválido ou inativo.");
            }

            var chamado = new Chamado
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Prioridade = dto.Prioridade,
                SolicitanteId = solicitanteId,
                Status = StatusChamado.Aberto,
                DataAbertura = DateTime.UtcNow
            };

            _context.Chamados.Add(chamado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetChamado),
                new { id = chamado.Id },
                new
                {
                    chamado.Id,
                    chamado.Titulo,
                    chamado.Descricao,
                    chamado.Status,
                    chamado.Prioridade,
                    chamado.SolicitanteId,
                    chamado.AtendenteId,
                    chamado.DataAbertura,
                    chamado.DataFechamento
                });
        }

        // PUT: api/Chamados/{id}
        // Admin atualiza somente título, descrição e prioridade.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutChamado(
            Guid id,
            [FromBody] AtualizarChamadoDto dto)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            chamado.Titulo = dto.Titulo;
            chamado.Descricao = dto.Descricao;
            chamado.Prioridade = dto.Prioridade;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Chamados/{id}
        // Exclusão restrita ao Admin.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteChamado(Guid id)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            _context.Chamados.Remove(chamado);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Chamados/{id}/atribuir?atendenteId=...
        // Atendente assume o chamado somente para si.
        [HttpPut("{id}/atribuir")]
        [Authorize(Roles = "Atendente")]
        public async Task<IActionResult> AtribuirChamado(
            Guid id,
            [FromQuery] Guid atendenteId)
        {
            if (!TentarObterUsuarioId(out var usuarioId))
            {
                return Unauthorized();
            }

            if (atendenteId != usuarioId)
            {
                return Forbid();
            }

            var atendente = await _context.Usuarios
                .FindAsync(usuarioId);

            if (atendente == null ||
                !atendente.Ativo ||
                atendente.Papel != Papel.Atendente)
            {
                return Forbid();
            }

            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            chamado.AtendenteId = usuarioId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Chamados/{id}/status
        // Atendente altera o status respeitando as transições.
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Atendente")]
        public async Task<IActionResult> AlterarStatus(
            Guid id,
            [FromBody] AlterarStatusDto dto)
        {
            var chamado = await _context.Chamados
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chamado == null)
            {
                return NotFound(new
                {
                    mensagem = "Chamado não encontrado."
                });
            }

            var statusAtual = chamado.Status;
            var novoStatus = dto.Status;

            if (statusAtual == novoStatus)
            {
                return BadRequest(new
                {
                    mensagem = "O chamado já está no status informado.",
                    statusAtual = statusAtual.ToString(),
                    transicoesPermitidas =
                        TransicoesStatus.ObterTransicoesPermitidas(
                            statusAtual)
                });
            }

            if (!TransicoesStatus.PodeAlterar(statusAtual, novoStatus))
            {
                return BadRequest(new
                {
                    mensagem =
                        $"Transição de status inválida de {statusAtual} para {novoStatus}.",
                    statusAtual = statusAtual.ToString(),
                    transicoesPermitidas =
                        TransicoesStatus.ObterTransicoesPermitidas(
                            statusAtual)
                });
            }

            chamado.Status = novoStatus;

            if (novoStatus == StatusChamado.Fechado)
            {
                chamado.DataFechamento = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensagem =
                    $"Status alterado de {statusAtual} para {novoStatus}.",
                chamado.Id,
                chamado.Titulo,
                Status = chamado.Status.ToString(),
                chamado.DataFechamento
            });
        }
    }
}