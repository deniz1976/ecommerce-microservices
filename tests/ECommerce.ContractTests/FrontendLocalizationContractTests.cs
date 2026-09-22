using System.Text.RegularExpressions;

namespace ECommerce.ContractTests;

public sealed class FrontendLocalizationContractTests
{
    [Fact]
    public void TurkishDictionaryUsesTurkishCharactersForUserFacingCopy()
    {
        string dictionary = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Frontend",
            "lib",
            "i18n",
            "dictionaries.ts"));
        int start = dictionary.IndexOf("const tr: Dictionary", StringComparison.Ordinal);
        int end = dictionary.IndexOf("export const dictionaries", start, StringComparison.Ordinal);
        string turkish = dictionary[start..end];

        Assert.Contains("Mağazalarınız", turkish, StringComparison.Ordinal);
        Assert.Contains("Satıcı çalışma alanı", turkish, StringComparison.Ordinal);
        Assert.Contains("Ürün başarıyla oluşturuldu.", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Magazalariniz", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Satici", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Urun", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Kullanici", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Gorsel", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Ã", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Ä", turkish, StringComparison.Ordinal);
        Assert.DoesNotContain("Å", turkish, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("seller", "product-create-form.tsx")]
    [InlineData("seller", "product-edit-form.tsx")]
    [InlineData("seller", "seller-product-management-page.tsx")]
    [InlineData("customer", "customer-dashboard.tsx")]
    [InlineData("customer", "product-detail.tsx")]
    [InlineData("admin", "admin-catalog-workspace.tsx")]
    public void LocaleSensitiveCatalogViewsReloadServerData(string area, string fileName)
    {
        string source = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Frontend",
            "components",
            area,
            fileName));

        Assert.Contains(
            DependencyArrays(source),
            dependencies => dependencies.Contains("locale"));
    }

    private static IEnumerable<string[]> DependencyArrays(string source)
    {
        return Regex
            .Matches(source, @"\}, \[(?<dependencies>[^\]]*)\]\)", RegexOptions.None, TimeSpan.FromSeconds(5))
            .Select(match => match.Groups["dependencies"].Value
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null &&
            !File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException(
                "Repository root containing ECommerce.sln was not found.");
    }
}
