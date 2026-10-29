using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Chamado>>> GetChamados()
        {
            var chamados = await _context.Chamados
                .Include(c => c.Solicitante)
                .Include(c => c.Atendente)
                .ToListAsync();

            return Ok(chamados);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Chamado>> GetChamado(Guid id)
        {
            var chamado = await _context.Chamados
                .Include(c => c.Solicitante)
                .Include(c => c.Atendente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (chamado == null)
            {
                return NotFound();
            }

            return Ok(chamado);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Chamado>> PostChamado(Chamado chamado)
        {
            var solicitante = await _context.Usuarios.FindAsync(chamado.SolicitanteId);

            if (solicitante == null || !solicitante.Ativo)
            {
                return BadRequest("Solicitante inválido ou inativo.");
            }

            chamado.DataAbertura = DateTime.UtcNow;
            chamado.Status = StatusChamado.Aberto;

            _context.Chamados.Add(chamado);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetChamado), new { id = chamado.Id }, chamado);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutChamado(Guid id, Chamado chamado)
        {
            if (id != chamado.Id)
            {
                return BadRequest();
            }

            _context.Entry(chamado).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Chamados.Any(e => e.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

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

        [HttpPut("{id}/atribuir")]
        [Authorize(Roles = "Atendente")]
        public async Task<IActionResult> AtribuirChamado(Guid id, Guid atendenteId)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            var atendente = await _context.Usuarios.FindAsync(atendenteId);

            if (atendente == null || !atendente.Ativo)
            {
                return BadRequest("Atendente inválido ou inativo.");
            }

            chamado.AtendenteId = atendenteId;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Atendente")]
        public async Task<IActionResult> AlterarStatus(Guid id, [FromBody] AlterarStatusDto dto)
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
                    transicoesPermitidas = TransicoesStatus.ObterTransicoesPermitidas(statusAtual)
                });
            }

            if (!TransicoesStatus.PodeAlterar(statusAtual, novoStatus))
            {
                return BadRequest(new
                {
                    mensagem = $"Transição de status inválida de {statusAtual} para {novoStatus}.",
                    statusAtual = statusAtual.ToString(),
                    transicoesPermitidas = TransicoesStatus.ObterTransicoesPermitidas(statusAtual)
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
                mensagem = $"Status alterado de {statusAtual} para {novoStatus}.",
                chamado.Id,
                chamado.Titulo,
                Status = chamado.Status.ToString(),
                chamado.DataFechamento
            });
        }
    }
}