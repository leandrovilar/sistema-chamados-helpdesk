using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SistemaChamados.Data;
using SistemaChamados.Models;
using Microsoft.AspNetCore.Authorization;

namespace SistemaChamados.Controllers;

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

    //GET: api/comentarios
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Comentario>>> GetComentarios()
    {
        return await _context.Comentarios
            .Include(c => c.Chamado)
            .Include(c => c.Usuario)
            .ToListAsync();
    }


    //GET: api/comentarios/5
    [HttpGet("id")]
    public async Task<ActionResult<Comentario>> GetComentario(Guid id)
    {
        var comentario = await _context.Comentarios
        .Include(c => c.Chamado)
        .Include(c => c.Usuario)
        .FirstOrDefaultAsync(c => c.Id == id);

        if (comentario == null)
        {
            return NotFound();
        }
        return comentario;
    }

    // POST: api/comentarios
    [HttpPost]
    public async Task<ActionResult<Comentario>> PostComentario(Comentario comentario)
    {
        //Validação: verificar se o chamado existe
        var chamado = await _context.Chamados.FindAsync(comentario.ChamadoId);
        if (chamado == null)
        {
            return BadRequest("Chamado inválido");
        }

        //Validação: verificar se o usuário existe e está ativo
        var usuario = await _context.Usuarios.FindAsync(comentario.UsuarioId);
        if (usuario == null || !usuario.Ativo)
        {
            return BadRequest("Chamado inválido.");
        }

        //Definir data do comentário automaticamente
        comentario.DataComentario = DateTime.UtcNow;

        _context.Comentarios.Add(comentario);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetComentario), new { id = comentario.Id }, comentario);
    }
    //PUT: api/comentarios/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutComentario(Guid id, Comentario comentario)
    {
        if (id != comentario.Id)
        {
            return BadRequest();
        }

        _context.Entry(comentario).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Comentarios.Any(e => e.Id == id))
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


    // DELETE: api/comentarios/5
    [HttpDelete("{id}")]
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

    //GET: api/chamados/5/comentarios
    // [HttpGet("chamados/chamadosId")]
    // public async Task<ActionResult<IEnumerable<Comentario>>> GetComentariosPorChamado(Guid chamadoId)
    // {
    //     return await _context.Comentarios
    //     .Include(c => c.Usuario)
    //     .Where(c => c.ChamadoId == chamadoId)
    //     .ToListAsync();
    // }

    [HttpGet("chamados/{chamadoId}")]
    public async Task<ActionResult<IEnumerable<Comentario>>> GetComentariosPorChamado(
    [FromRoute] Guid chamadoId)
    {
        var comentarios = await _context.Comentarios
            .Include(c => c.Usuario)
            .Where(c => c.ChamadoId == chamadoId)
            .ToListAsync();

        return Ok(comentarios);
    }
}
