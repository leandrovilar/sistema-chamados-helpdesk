using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using SistemaChamados.Data;
using SistemaChamados.Models;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Options;

namespace SistemaChamados.Tests;

public class ChamadosControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ChamadosControllerTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase("ChamadosTestDatabase"));
            });
        });
    }

    [Fact]
    public async Task PostChamado_ComSolicitanteValido_DeveRetornar201()
    {
        var client = _factory.CreateClient();

        var usuario = new Usuario
        {
            Nome = "Maria Souza",
            Email = "maria.souza@email.com",
            Papel = Papel.Solicitante,
            Ativo = true
        };

        await client.PostAsJsonAsync("/api/usuarios", usuario);

        var chamado = new
        {
            titulo = "Computador não liga",
            descricao = "Ao pressionar o botão de energia, o computador não inicia.",
            prioridade = 2,
            solicitanteId = usuario.Id
        };

        var response = await client.PostAsJsonAsync("/api/chamados", chamado);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostChamado_ComSolicitanteInexistente_DeveRetornar400()
    {
        var client = _factory.CreateClient();

        var chamado = new
        {
            titulo = "Computador não liga",
            descricao = "Ao pressionar o botão de energia, o computador não inicia.",
            prioridade = 2,
            solicitanteId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/api/chamados", chamado);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AtribuirChamado_ComAtendenteValido_DeveRetornar204()
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

        var atendente = new Usuario
        {
            Nome = "Carlos Lima",
            Email = "carlos.lima@email.com",
            Papel = (Papel)1,
            Ativo = true
        };

        await client.PostAsJsonAsync("/api/usuarios", atendente);

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

        var response = await client.PutAsync(
            $"/api/chamados/{chamadoCriado!.Id}/atribuir?atendenteId={atendente.Id}",
            null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}