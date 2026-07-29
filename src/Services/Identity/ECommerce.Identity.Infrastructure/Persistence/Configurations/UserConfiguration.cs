using ECommerce.Identity.Domain;
using ECommerce.BuildingBlocks.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ExternalProvider).HasColumnName("external_provider").HasMaxLength(64);
        builder.Property(x => x.ExternalSubject).HasColumnName("external_subject").HasMaxLength(256);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsUtcTimestamp();
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at").IsUtcTimestamp();
        builder.Property(x => x.OnboardingCompletedAt).HasColumnName("onboarding_completed_at").IsUtcTimestamp();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => new { x.ExternalProvider, x.ExternalSubject }).IsUnique();
        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
