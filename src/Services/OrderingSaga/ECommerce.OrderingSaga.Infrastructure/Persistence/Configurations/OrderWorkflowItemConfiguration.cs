using ECommerce.OrderingSaga.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.OrderingSaga.Infrastructure.Persistence.Configurations;

public sealed class OrderWorkflowItemConfiguration : IEntityTypeConfiguration<OrderWorkflowItem>
{
    public void Configure(EntityTypeBuilder<OrderWorkflowItem> builder)
    {
        builder.ToTable("order_workflow_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.WorkflowId).HasColumnName("workflow_id");
        builder.Property(x => x.ProductId).HasColumnName("product_id");
        builder.Property(x => x.Quantity).HasColumnName("quantity");
    }
}
