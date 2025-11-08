using AutoMapper;
using LMOrders.Application.DTOs;
using LMOrders.Application.Interfaces.Integrations;
using LMOrders.Application.Interfaces.Services;
using LMOrders.Domain.Entities;
using LMOrders.Domain.Exceptions;
using LMOrders.Domain.Interfaces.Repositories;

namespace LMOrders.Application.Services;

public class PedidoApplicationService : IPedidoApplicationService
{
    private readonly IMapper _mapper;
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IBillingIntegrationService _billingIntegrationService;

    public PedidoApplicationService(
        IMapper mapper,
        IPedidoRepository pedidoRepository,
        IBillingIntegrationService billingIntegrationService)
    {
        _mapper = mapper;
        _pedidoRepository = pedidoRepository;
        _billingIntegrationService = billingIntegrationService;
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

        await _billingIntegrationService.NotifyAsync(pedido, cancellationToken);

        return _mapper.Map<PedidoResponse>(pedido);
    }

    public async Task<PedidoResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id, cancellationToken);

        return pedido is null ? null : _mapper.Map<PedidoResponse>(pedido);
    }
}

