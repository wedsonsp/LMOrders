using LMOrders.Domain.Entities;
using LMOrders.Domain.Interfaces.Repositories;
using LMOrders.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace LMOrders.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly PedidosDbContext _dbContext;

    public PedidoRepository(PedidosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        await _dbContext.Pedidos.AddAsync(pedido, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Pedido?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Pedidos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _dbContext.Pedidos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (pedido is null)
        {
            return;
        }

        _dbContext.Pedidos.Remove(pedido);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}

