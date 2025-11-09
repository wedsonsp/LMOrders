using LMOrders.Domain.Entities;
using LMOrders.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMOrders.Infrastructure.Persistence.Configurations;

public class PedidoConfiguration : IEntityTypeConfiguration<Pedido>
{
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(pedido => pedido.Id);

        builder.Property(pedido => pedido.Id)
            .ValueGeneratedOnAdd();

        builder.Property(pedido => pedido.ClienteId)
            .IsRequired();

        builder.Property(pedido => pedido.DataPedido)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(pedido => pedido.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(pedido => pedido.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Property(pedido => pedido.Status)
            .HasConversion(
                status => status.ToString(),
                value => Enum.Parse<PedidoStatus>(value))
            .HasMaxLength(64)
            .IsRequired();

        builder.Ignore(pedido => pedido.ValorTotal);
        builder.Ignore(pedido => pedido.Itens);
    }
}

