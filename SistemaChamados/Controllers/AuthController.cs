using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    public AuthController(ChamadosContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequestDto request)
    {
        var usuario = _context.Usuarios
            .FirstOrDefault(u =>
                u.Email.ToLower() == request.Email.ToLower() &&
                u.Senha == request.Senha &&
                u.Ativo);

        if (usuario == null)
        {
            return Unauthorized(new
            {
                mensagem = "E-mail ou senha inválidos."
            });
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

        // var claims = new[]
        // {
        //     new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
        //     new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
        //     new Claim(ClaimTypes.Role, usuario.Papel.ToString()),
        //     new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        // };

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