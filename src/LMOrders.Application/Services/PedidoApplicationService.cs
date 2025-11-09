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
    private readonly IPedidoItemRepository _pedidoItemRepository;
    private readonly IBillingIntegrationService _billingIntegrationService;

    public PedidoApplicationService(
        IMapper mapper,
        IPedidoRepository pedidoRepository,
        IPedidoItemRepository pedidoItemRepository,
        IBillingIntegrationService billingIntegrationService)
    {
        _mapper = mapper;
        _pedidoRepository = pedidoRepository;
        _pedidoItemRepository = pedidoItemRepository;
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

        return _mapper.Map<PedidoResponse>(pedido);
    }

    public async Task<PedidoResponse?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
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

        return _mapper.Map<PedidoResponse>(pedido);
    }
}

