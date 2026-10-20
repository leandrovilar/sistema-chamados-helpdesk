using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SistemaChamados.Data;
using SistemaChamados.Models;

namespace SistemaChamados.Tests;

public class ComentariosControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ComentariosControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var optionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ChamadosContext>));

                if (optionsDescriptor != null)
                {
                    services.Remove(optionsDescriptor);
                }

                var configurationDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ChamadosContext>));

                if (configurationDescriptor != null)
                {
                    services.Remove(configurationDescriptor);
                }

                services.AddDbContext<ChamadosContext>(options =>
                    options.UseInMemoryDatabase("ComentariosTestDatabase"));
            });
        });
    }

    [Fact]
    public async Task PostComentario_ComDadosValidos_DeveRetornar201()
    {
        var client = _factory.CreateClient();

        var solicitante = new Usuario
        {
            Nome = "Maria Souza",
            Email = "maria.souza@email.com",
            Papel = Papel.Solicitante,
            Ativo = true
        };

        await client.PostAsJsonAsync("/api/usuarios", solicitante);

        var chamado = new
        {
            titulo = "Computador não liga",
            descricao = "Ao pressionar o botão de energia, o computador não inicia.",
            prioridade = 2,
            solicitanteId = solicitante.Id
        };

        var chamadoResponse = await client.PostAsJsonAsync("/api/chamados", chamado);

        var chamadoCriado = await chamadoResponse.Content
            .ReadFromJsonAsync<Chamado>();

        var comentario = new
        {
            chamadoId = chamadoCriado!.Id,
            usuarioId = solicitante.Id,
            conteudo = "Chamado recebido. Vou verificar o equipamento."
        };

        var response = await client.PostAsJsonAsync("/api/comentarios", comentario);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostComentario_ComChamadoInexistente_DeveRetornar400()
    {
        var client = _factory.CreateClient();

        var usuario = new Usuario
        {
            Nome = "Carlos Lima",
            Email = "carlos.lima@email.com",
            Papel = (Papel)1,
            Ativo = true
        };

        await client.PostAsJsonAsync("/api/usuarios", usuario);

        var comentario = new
        {
            chamadoId = Guid.NewGuid(),
            usuarioId = usuario.Id,
            conteudo = "Comentário para chamado inexistente."
        };

        var response = await client.PostAsJsonAsync("/api/comentarios", comentario);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetComentariosPorChamado_DeveRetornar200()
    {
        var client = _factory.CreateClient();

        var solicitante = new Usuario
        {
            Nome = "Maria Souza",
            Email = "maria.souza@email.com",
            Papel = Papel.Solicitante,
            Ativo = true
        };

        await client.PostAsJsonAsync("/api/usuarios", solicitante);

        var chamado = new
        {
            titulo = "Computador não liga",
            descricao = "Ao pressionar o botão de energia, o computador não inicia.",
            prioridade = 2,
            solicitanteId = solicitante.Id
        };

        var chamadoResponse = await client.PostAsJsonAsync("/api/chamados", chamado);

        var chamadoCriado = await chamadoResponse.Content
            .ReadFromJsonAsync<Chamado>();

        var comentario = new
        {
            chamadoId = chamadoCriado!.Id,
            usuarioId = solicitante.Id,
            conteudo = "Chamado recebido. Vou verificar o equipamento."
        };

        await client.PostAsJsonAsync("/api/comentarios", comentario);

        var response = await client.GetAsync(
            $"/api/comentarios/chamados/{chamadoCriado.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}