using AutoMapper;
using LMOrders.Application.DTOs;
using LMOrders.Application.Events;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Application.Interfaces.Services;
using LMOrders.Domain.Entities;
using LMOrders.Domain.Exceptions;
using LMOrders.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace LMOrders.Application.Services;

public class PedidoApplicationService : IPedidoApplicationService
{
    private readonly IMapper _mapper;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPedidoItemRepository _pedidoItemRepository;
    private readonly IBillingIntegrationService _billingIntegrationService;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly IKafkaTopicResolver _kafkaTopicResolver;
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions CacheSerializerOptions = new(JsonSerializerDefaults.Web);
    private static readonly DistributedCacheEntryOptions CacheEntryOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    };

    public PedidoApplicationService(
        IMapper mapper,
        IPedidoRepository pedidoRepository,
        IPedidoItemRepository pedidoItemRepository,
        IBillingIntegrationService billingIntegrationService,
        IKafkaProducerService kafkaProducerService,
        IKafkaTopicResolver kafkaTopicResolver,
        IDistributedCache cache)
    {
        _mapper = mapper;
        _pedidoRepository = pedidoRepository;
        _pedidoItemRepository = pedidoItemRepository;
        _billingIntegrationService = billingIntegrationService;
        _kafkaProducerService = kafkaProducerService;
        _kafkaTopicResolver = kafkaTopicResolver;
        _cache = cache;
    }

    public async Task<PedidoResponse> CriarAsync(CriarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.ClienteId <= 0)
        {
            throw new DomainException("O ClienteId é obrigatório.");
        }

        if (request.Itens is null || request.Itens.Count == 0)
        {
            throw new DomainException("O pedido deve possuir ao menos um item.");
        }

        var itens = request.Itens.Select(item =>
            new PedidoItem(item.ProdutoId, item.Produto, item.Quantidade, item.ValorUnitario)).ToList();

        var dataPedido = request.DataPedido == default ? DateTime.UtcNow : request.DataPedido;

        var pedido = new Pedido(request.ClienteId, dataPedido, itens);

        await _pedidoRepository.AdicionarAsync(pedido, cancellationToken);

        try
        {
            foreach (var item in pedido.Itens)
            {
                item.AssociarPedido(pedido.Id);
            }

            await _pedidoItemRepository.SalvarItensAsync(pedido.Id, pedido.Itens, cancellationToken);
        }
        catch
        {
            await _pedidoRepository.RemoverAsync(pedido.Id, cancellationToken);
            throw;
        }

        await _billingIntegrationService.NotifyAsync(pedido, cancellationToken);

        var evento = new PedidoCriadoEvent(
            pedido.Id,
            pedido.ClienteId,
            pedido.DataPedido,
            pedido.ValorTotal,
            pedido.Itens.Select(item => new PedidoCriadoItemEvent(item.ProdutoId, item.Produto, item.Quantidade, item.ValorUnitario)).ToList());

        var topic = _kafkaTopicResolver.GetPedidoCriadoTopic();
        await _kafkaProducerService.ProduceAsync(topic, evento, cancellationToken);

        var response = _mapper.Map<PedidoResponse>(pedido);
        await CacheResponseAsync(response, cancellationToken);

        return response;
    }

    public async Task<PedidoResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(id);
        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);

        if (!string.IsNullOrWhiteSpace(cached))
        {
            var cachedResponse = JsonSerializer.Deserialize<PedidoResponse>(cached, CacheSerializerOptions);
            if (cachedResponse is not null)
            {
                return cachedResponse;
            }
        }

        var pedido = await _pedidoRepository.ObterPorIdAsync(id, cancellationToken);

        if (pedido is null)
        {
            return null;
        }

        var itens = await _pedidoItemRepository.ObterPorPedidoIdAsync(id, cancellationToken);

        foreach (var item in itens)
        {
            pedido.AdicionarItem(item);
        }

        var response = _mapper.Map<PedidoResponse>(pedido);
        await CacheResponseAsync(response, cancellationToken);

        return response;
    }

    private async Task CacheResponseAsync(PedidoResponse response, CancellationToken cancellationToken)
    {
        var cacheKey = GetCacheKey(response.Id);
        var payload = JsonSerializer.Serialize(response, CacheSerializerOptions);
        await _cache.SetStringAsync(cacheKey, payload, CacheEntryOptions, cancellationToken);
    }

    private static string GetCacheKey(int pedidoId) => $"pedido:{pedidoId}";
}

