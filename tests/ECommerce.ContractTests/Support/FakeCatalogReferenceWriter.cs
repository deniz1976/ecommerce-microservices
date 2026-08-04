using ECommerce.BuildingBlocks.Contracts.Persistence;
using ECommerce.Catalog.Application.References;
using ECommerce.Catalog.Domain;

namespace ECommerce.ContractTests;

internal sealed class FakeCatalogReferenceWriter :
    IRepository<Category, Guid>,
    IRepository<Brand, Guid>,
    IUnitOfWork,
    ICatalogReferenceConflictReader
{
    private readonly Dictionary<Guid, Category> categories;
    private readonly Dictionary<Guid, Brand> brands;

    public FakeCatalogReferenceWriter(
        IEnumerable<Category>? categories = null,
        IEnumerable<Brand>? brands = null)
    {
        this.categories = (categories ?? []).ToDictionary(item => item.Id);
        this.brands = (brands ?? []).ToDictionary(item => item.Id);
        CategorySlugs.UnionWith(this.categories.Values.Select(item => item.Slug));
        BrandSlugs.UnionWith(this.brands.Values.Select(item => item.Slug));
    }

    public Category? AddedCategory { get; private set; }

    public Brand? AddedBrand { get; private set; }

    public HashSet<string> CategorySlugs { get; } = new(StringComparer.Ordinal);

    public HashSet<string> BrandSlugs { get; } = new(StringComparer.Ordinal);

    Task<Category?> IRepository<Category, Guid>.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        Task.FromResult(categories.GetValueOrDefault(id));

    Task<Brand?> IRepository<Brand, Guid>.GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        Task.FromResult(brands.GetValueOrDefault(id));

    void IRepository<Category, Guid>.Add(Category entity)
    {
        AddedCategory = entity;
        categories.Add(entity.Id, entity);
        CategorySlugs.Add(entity.Slug);
    }

    void IRepository<Brand, Guid>.Add(Brand entity)
    {
        AddedBrand = entity;
        brands.Add(entity.Id, entity);
        BrandSlugs.Add(entity.Slug);
    }

    public Task<bool> CategorySlugExistsAsync(
        string slug,
        Guid? excludedCategoryId,
        CancellationToken cancellationToken) =>
        Task.FromResult(categories.Values.Any(category =>
            category.Slug == slug &&
            (!excludedCategoryId.HasValue ||
             category.Id != excludedCategoryId.Value)));

    public Task<bool> BrandSlugExistsAsync(
        string slug,
        Guid? excludedBrandId,
        CancellationToken cancellationToken) =>
        Task.FromResult(brands.Values.Any(brand =>
            brand.Slug == slug &&
            (!excludedBrandId.HasValue ||
             brand.Id != excludedBrandId.Value)));

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
