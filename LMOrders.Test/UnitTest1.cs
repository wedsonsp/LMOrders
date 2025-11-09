using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using LMOrders.Test.Models;
using LMOrders.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LMOrders.Test;

public class PedidosEndpointsTests : IClassFixture<PedidoApiFactory>
{
    private readonly PedidoApiFactory _factory;
    private readonly HttpClient _client;

    public PedidosEndpointsTests(PedidoApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar201()
    {
        var request = new
        {
            clienteId = 123,
            dataPedido = DateTime.UtcNow,
            itens = new[]
            {
                new { produtoId = 10, produto = "Console Portátil", quantidade = 1, valorUnitario = 1999.99m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedido = await response.Content.ReadFromJsonAsync<PedidoResponseDto>(Serializer.Options);

        pedido.Should().NotBeNull();
        pedido!.Id.Should().BeGreaterThan(0);
        pedido.ClienteId.Should().Be(123);
        pedido.Itens.Should().HaveCount(1);
        pedido.ValorTotal.Should().Be(1999.99m);
    }

    [Fact]
    public async Task ObterPedido_DeveRetornarPedidoCriado()
    {
        var createRequest = new
        {
            clienteId = 321,
            dataPedido = DateTime.UtcNow,
            itens = new[]
            {
                new { produtoId = 5, produto = "Headset Gamer", quantidade = 2, valorUnitario = 350.50m }
            }
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/pedidos", createRequest);
        createResponse.EnsureSuccessStatusCode();

        var criado = await createResponse.Content.ReadFromJsonAsync<PedidoResponseDto>(Serializer.Options);
        criado.Should().NotBeNull();
        criado!.Id.Should().BeGreaterThan(0);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PedidosDbContext>();
            var existe = await db.Pedidos.Include(p => p.Itens).SingleOrDefaultAsync(p => p.Id == criado.Id);
            existe.Should().NotBeNull();
            existe!.Itens.Should().HaveCount(1);
        }

        var getResponse = await _client.GetAsync($"/api/v1/pedidos/{criado.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var pedido = await getResponse.Content.ReadFromJsonAsync<PedidoResponseDto>(Serializer.Options);

        pedido.Should().NotBeNull();
        pedido!.Id.Should().Be(criado.Id);
        pedido.ClienteId.Should().Be(321);
        pedido.Itens.Should().HaveCount(1);
        pedido.Itens.First().Produto.Should().Be("Headset Gamer");
    }

    [Fact]
    public async Task CriarPedido_SemItens_DeveRetornar400()
    {
        var request = new
        {
            clienteId = 10,
            dataPedido = DateTime.UtcNow,
            itens = Array.Empty<object>()
        };

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_ClienteIdInvalido_DeveRetornar400()
    {
        var request = new
        {
            clienteId = 0,
            dataPedido = DateTime.UtcNow,
            itens = new[]
            {
                new { produtoId = 10, produto = "Monitor", quantidade = 1, valorUnitario = 899.90m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_ItemQuantidadeInvalida_DeveRetornar400()
    {
        var request = new
        {
            clienteId = 55,
            dataPedido = DateTime.UtcNow,
            itens = new[]
            {
                new { produtoId = 9, produto = "Mouse Gamer", quantidade = 0, valorUnitario = 150m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/pedidos", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ObterPedido_Inexistente_DeveRetornar404()
    {
        var response = await _client.GetAsync("/api/v1/pedidos/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

internal static class Serializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };
}