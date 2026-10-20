using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SistemaChamados.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace SistemaChamados.Tests;

public class UsuariosControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UsuariosControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var sqlServerDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ChamadosContext>));

                if (sqlServerDescriptor != null)
                {
                    services.Remove(sqlServerDescriptor);
                }

                var configurationDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IDbContextOptionsConfiguration<ChamadosContext>));

                if (configurationDescriptor != null)
                {
                    services.Remove(configurationDescriptor);
                }

                services.AddDbContext<ChamadosContext>(options =>
                    options.UseInMemoryDatabase("UsuariosTestDatabase"));
            });
        });
    }

    [Fact]
    public async Task PostUsuario_ComDadosValidos_DeveRetornar201()
    {
        var client = _factory.CreateClient();

        var usuario = new
        {
            nome = "Maria Souza",
            email = "maria.souza@email.com",
            papel = 0,
            ativo = true
        };

        var response = await client.PostAsJsonAsync("/api/usuarios", usuario);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostUsuario_ComEmailInvalido_DeveRetornar400()
    {
        var client = _factory.CreateClient();

        var usuario = new
        {
            nome = "Maria Souza",
            email = "email-invalido",
            papel = 0,
            ativo = true
        };

        var response = await client.PostAsJsonAsync("/api/usuarios", usuario);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}