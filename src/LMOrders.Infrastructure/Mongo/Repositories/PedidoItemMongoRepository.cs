using LMOrders.Domain.Entities;
using LMOrders.Domain.Interfaces.Repositories;
using LMOrders.Infrastructure.Mongo.Documents;
using LMOrders.Infrastructure.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace LMOrders.Infrastructure.Mongo.Repositories;

public class PedidoItemMongoRepository : IPedidoItemRepository
{
    private readonly IMongoCollection<PedidoItensDocument> _collection;

    public PedidoItemMongoRepository(IOptions<MongoOptions> options, IMongoClient mongoClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(mongoClient);

        var settings = options.Value ?? throw new ArgumentNullException(nameof(options));

        if (string.IsNullOrWhiteSpace(settings.Database))
        {
            throw new ArgumentException("A base de dados do MongoDB não foi configurada.", nameof(options));
        }

        var database = mongoClient.GetDatabase(settings.Database);
        _collection = database.GetCollection<PedidoItensDocument>(settings.PedidoItensCollection);
    }

    public async Task SalvarItensAsync(int pedidoId, IEnumerable<PedidoItem> itens, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itens);

        var document = new PedidoItensDocument
        {
            PedidoId = pedidoId,
            Itens = itens.Select(item => new PedidoItemDocument
            {
                ProdutoId = item.ProdutoId,
                Produto = item.Produto,
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorUnitario
            }).ToList()
        };

        var filter = Builders<PedidoItensDocument>.Filter.Eq(doc => doc.PedidoId, pedidoId);
        await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);
    }

    public async Task<IReadOnlyCollection<PedidoItem>> ObterPorPedidoIdAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<PedidoItensDocument>.Filter.Eq(doc => doc.PedidoId, pedidoId);
        var document = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (document is null)
        {
            return Array.Empty<PedidoItem>();
        }

        var itens = document.Itens.Select(itemDocument =>
        {
            var item = new PedidoItem(itemDocument.ProdutoId, itemDocument.Produto, itemDocument.Quantidade, itemDocument.ValorUnitario);
            item.AssociarPedido(pedidoId);
            return item;
        }).ToList();

        return itens;
    }

    public async Task RemoverPorPedidoIdAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<PedidoItensDocument>.Filter.Eq(doc => doc.PedidoId, pedidoId);
        await _collection.DeleteOneAsync(filter, cancellationToken);
    }
}
