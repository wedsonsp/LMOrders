using LMOrders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LMOrders.Infrastructure.Persistence.Configurations;

public class PedidoItemConfiguration : IEntityTypeConfiguration<PedidoItem>
{
    public void Configure(EntityTypeBuilder<PedidoItem> builder)
    {
        builder.ToTable("PedidoItens");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .ValueGeneratedOnAdd();

        builder.Property(item => item.PedidoId)
            .IsRequired();

        builder.Property(item => item.ProdutoId)
            .IsRequired();

        builder.Property(item => item.Produto)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.Quantidade)
            .IsRequired();

        builder.Property(item => item.ValorUnitario)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(item => item.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(item => item.UpdatedAt)
            .HasColumnType("datetime2");

        builder.Ignore(item => item.Total);
    }
}

