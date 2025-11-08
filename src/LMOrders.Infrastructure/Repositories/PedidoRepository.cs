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
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}

