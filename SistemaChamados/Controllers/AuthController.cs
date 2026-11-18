using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SistemaChamados.Data;
using SistemaChamados.Dtos;
using SistemaChamados.Models;

namespace SistemaChamados.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ChamadosContext _context;
    private readonly IConfiguration _configuration;

    // AJUSTE: ferramenta para verificar a senha.
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AuthController(
        ChamadosContext context,
        IConfiguration configuration,
        IPasswordHasher<Usuario> passwordHasher)
    {
        _context = context;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequestDto request)
    {
        // AJUSTE: a consulta não compara mais a senha.
        var usuario = _context.Usuarios
            .FirstOrDefault(u =>
                u.Email.ToLower() == request.Email.ToLower() &&
                u.Ativo);

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensagem = "E-mail ou senha inválidos."
            });
        }

        // AJUSTE: verifica a senha recebida usando o hash armazenado.
        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.Senha,
            request.Senha);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                mensagem = "E-mail ou senha inválidos."
            });
        }

        // Atualiza o hash caso a ferramenta indique essa necessidade.
        if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.Senha = _passwordHasher.HashPassword(
                usuario,
                request.Senha);

            _context.SaveChanges();
        }

        var token = GerarToken(usuario);

        return Ok(new
        {
            token,
            usuario = new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                Papel = usuario.Papel.ToString()
            }
        });
    }

    private string GerarToken(Usuario usuario)
    {
        var jwt = _configuration.GetSection("Jwt");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwt["Key"]!));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Papel.ToString())
        };

        var token = new JwtSecurityToken(
        issuer: jwt["Issuer"],
        audience: jwt["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(
            int.Parse(jwt["ExpiryMinutes"]!)),
        signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}