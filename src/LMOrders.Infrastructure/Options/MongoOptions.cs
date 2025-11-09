namespace LMOrders.Infrastructure.Options;

public class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; init; } = string.Empty;
    public string Database { get; init; } = string.Empty;
    public string PedidoItensCollection { get; init; } = "pedidoItens";
}
