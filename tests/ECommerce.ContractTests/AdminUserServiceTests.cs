using ECommerce.ContractTests.Support;
using ECommerce.Identity.Application.AdminUsers;
using ECommerce.Identity.Application.Queries.SearchAdminUsers;

namespace ECommerce.ContractTests;

public sealed class SearchAdminUsersQueryHandlerTests
{
    [Fact]
    public async Task SearchNormalizesPagingAndOptionalFilters()
    {
        TrackingAdminUserReader reader = new();
        SearchAdminUsersQueryHandler handler = new(new AdminUserService(reader));

        await handler.HandleAsync(
            new SearchAdminUsersQuery(0, 500, "  example  ", "  Seller  "),
            CancellationToken.None);

        Assert.NotNull(reader.LastQuery);
        Assert.Equal(1, reader.LastQuery.PageNumber);
        Assert.Equal(50, reader.LastQuery.PageSize);
        Assert.Equal("example", reader.LastQuery.Search);
        Assert.Equal("Seller", reader.LastQuery.Role);
    }

    [Fact]
    public async Task SearchBoundsDeepPagingAndInputLength()
    {
        TrackingAdminUserReader reader = new();
        SearchAdminUsersQueryHandler handler = new(new AdminUserService(reader));

        await handler.HandleAsync(
            new SearchAdminUsersQuery(int.MaxValue, 20, new string('a', 200), new string('b', 100)),
            CancellationToken.None);

        Assert.NotNull(reader.LastQuery);
        Assert.Equal(10_000, reader.LastQuery.PageNumber);
        Assert.Equal(100, reader.LastQuery.Search!.Length);
        Assert.Equal(32, reader.LastQuery.Role!.Length);
    }
}
