using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaChamados.Data;
using SistemaChamados.Models;

namespace SistemaChamados.Controllers;

[ApiController]
[Route("api/[controller]")]

public class ChamadosController : ControllerBase
{
    private readonly ChamadosContext _context;

    public ChamadosController(ChamadosContext context)
    {
        _context = context;
    }

    //GET: api/chamados 
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Chamado>>> Getchamados()
    {
        return await _context.Chamados
        .Include(c => c.Solicitante)
        .Include(c => c.Atendente)
        .ToListAsync();
    }

    //GET: api/chamados/5
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
        return chamado;
    }
    // POST: api/chamados
    [HttpPost]
    public async Task<ActionResult<Chamado>> PostChamado(Chamado chamado)
    {
        //Validação: verificar se o solicitante existe e está ativo
        var solicitante = await _context.Usuarios.FindAsync(chamado.SolicitanteId);
        if (solicitante == null || !solicitante.Ativo)
        {
            return BadRequest("Solicitante inválido ou inativo.");
        }

        //Definir data de abertura automaticamente
        chamado.DataAbertura = DateTime.UtcNow;
        chamado.Status = StatusChamado.Aberto;

        _context.Chamados.Add(chamado);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChamado), new { id = chamado.Id }, chamado);
    }


    //PUT: api/chamado/5
    [HttpPut("{id}")]
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
            else
            {
                throw;
            }
        }
        return NoContent();
    }

    // DELETE: api/chamados/5
    [HttpDelete("{id}")]
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

    //PUT: api/chamados/5/atribuir
    [HttpPut("{id}/atribuir")]
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

    //PUT: api/chamados/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> MudarStatus(Guid id, [FromBody] int status)
    {
        var chamado = await _context.Chamados.FindAsync(id);
        if (chamado == null)
        {
            return NotFound();
        }

        // Validação simples de máquina de estados
        if (status < 0 || status > 3)
        {
            return BadRequest("Status inválido.");
        }

        chamado.Status = (StatusChamado)status;

        //Se for fechado, definir data de fechamento
        if (chamado.Status == StatusChamado.Fechado)
        {
            chamado.DataFechamento = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

}
