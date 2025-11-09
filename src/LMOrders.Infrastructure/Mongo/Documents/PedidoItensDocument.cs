using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LMOrders.Infrastructure.Mongo.Documents;

public class PedidoItensDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("pedidoId")]
    public int PedidoId { get; set; }

    [BsonElement("itens")]
    public List<PedidoItemDocument> Itens { get; set; } = new();
}

public class PedidoItemDocument
{
    [BsonElement("produtoId")]
    public int ProdutoId { get; set; }

    [BsonElement("produto")]
    public string Produto { get; set; } = string.Empty;

    [BsonElement("quantidade")]
    public int Quantidade { get; set; }

    [BsonElement("valorUnitario")]
    public decimal ValorUnitario { get; set; }
}
