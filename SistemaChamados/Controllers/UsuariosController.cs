using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using SistemaChamados.Data;
using SistemaChamados.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace SistemaChamados.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly ChamadosContext _context;

    // AJUSTE: guarda a ferramenta que será usada para tratar senhas.
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public UsuariosController(
        ChamadosContext context,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // GET: api/usuarios
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsuarios()
    {
        // AJUSTE: seleciona somente os campos permitidos na resposta.
        var usuarios = await _context.Usuarios
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                Papel = u.Papel.ToString(),
                u.Ativo
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    // GET: api/usuarios/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUsuario(Guid id)
    {
        // AJUSTE: seleciona os dados do usuário sem incluir Senha.
        var usuario = await _context.Usuarios
            .Where(u => u.Id == id)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                Papel = u.Papel.ToString(),
                u.Ativo
            })
            .FirstOrDefaultAsync();

        if (usuario == null)
        {
            return NotFound();
        }

        return Ok(usuario);
    }

    /// POST: api/usuarios
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Senha))
        {
            return BadRequest("A senha é obrigatória.");
        }

        // AJUSTE: substitui a senha recebida pelo hash antes de salvar.
        usuario.Senha = _passwordHasher.HashPassword(
            usuario,
            usuario.Senha);

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // AJUSTE: devolve somente os dados necessários, sem senha ou hash.
        return CreatedAtAction(
            nameof(GetUsuario),
            new { id = usuario.Id },
            new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                Papel = usuario.Papel.ToString(),
                usuario.Ativo
            });
    }

    // PUT: api/usuarios/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PutUsuario(Guid id, Usuario usuario)
    {
        if (id != usuario.Id)
        {
            return BadRequest("O ID da URL deve ser igual ao ID do usuário.");
        }

        if (string.IsNullOrWhiteSpace(usuario.Senha))
        {
            return BadRequest("A senha é obrigatória.");
        }

        // AJUSTE: busca o usuário existente no banco.
        var usuarioExistente = await _context.Usuarios.FindAsync(id);

        if (usuarioExistente == null)
        {
            return NotFound();
        }

        // AJUSTE: atualiza explicitamente os campos desta operação.
        usuarioExistente.Nome = usuario.Nome;
        usuarioExistente.Email = usuario.Email;
        usuarioExistente.Papel = usuario.Papel;
        usuarioExistente.Ativo = usuario.Ativo;

        // AJUSTE: armazena o hash da senha informada.
        usuarioExistente.Senha = _passwordHasher.HashPassword(
            usuarioExistente,
            usuario.Senha);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/usuarios/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUsuario(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
