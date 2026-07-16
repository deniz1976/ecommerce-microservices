using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ECommerce.Basket.Infrastructure.Persistence.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContext(typeof(BasketDbContext))]
    partial class BasketDbContextModelSnapshot : Microsoft.EntityFrameworkCore.Infrastructure.ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ECommerce.Basket.Domain.BasketCheckoutSnapshot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)")
                        .HasColumnName("currency");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid")
                        .HasColumnName("customer_id");

                    b.Property<decimal>("TotalAmount")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("total_amount");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt");

                    b.HasIndex("CustomerId");

                    b.ToTable("basket_checkout_snapshots", (string)null);
                });

            modelBuilder.Entity("ECommerce.Basket.Domain.BasketCheckoutSnapshotItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<Guid>("BasketCheckoutSnapshotId")
                        .HasColumnType("uuid")
                        .HasColumnName("basket_checkout_snapshot_id");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("character varying(3)")
                        .HasColumnName("currency");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uuid")
                        .HasColumnName("product_id");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasColumnName("product_name");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer")
                        .HasColumnName("quantity");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)")
                        .HasColumnName("unit_price");

                    b.HasKey("Id");

                    b.HasIndex("BasketCheckoutSnapshotId");

                    b.HasIndex("ProductId");

                    b.ToTable("basket_checkout_snapshot_items", (string)null);
                });

            modelBuilder.Entity("ECommerce.Basket.Domain.BasketCheckoutSnapshotItem", b =>
                {
                    b.HasOne("ECommerce.Basket.Domain.BasketCheckoutSnapshot", "BasketCheckoutSnapshot")
                        .WithMany("Items")
                        .HasForeignKey("BasketCheckoutSnapshotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BasketCheckoutSnapshot");
                });

            modelBuilder.Entity("ECommerce.Basket.Domain.BasketCheckoutSnapshot", b =>
                {
                    b.Navigation("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
