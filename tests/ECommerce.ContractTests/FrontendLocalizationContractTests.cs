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

    [Fact]
    public void LocaleSensitiveCatalogViewsReloadServerData()
    {
        string frontendRoot = Path.Combine(FindRepositoryRoot(), "src", "Frontend", "components");

        Assert.Contains(
            "}, [locale])",
            File.ReadAllText(Path.Combine(frontendRoot, "seller", "product-create-form.tsx")),
            StringComparison.Ordinal);
        Assert.Contains(
            "}, [locale])",
            File.ReadAllText(Path.Combine(frontendRoot, "seller", "product-edit-form.tsx")),
            StringComparison.Ordinal);
        Assert.Contains(
            "}, [locale, selectedStoreId])",
            File.ReadAllText(Path.Combine(frontendRoot, "seller", "seller-product-management-page.tsx")),
            StringComparison.Ordinal);
        Assert.Contains(
            "}, [locale, query])",
            File.ReadAllText(Path.Combine(frontendRoot, "customer", "customer-dashboard.tsx")),
            StringComparison.Ordinal);
        Assert.Contains(
            "}, [locale, params.id])",
            File.ReadAllText(Path.Combine(frontendRoot, "customer", "product-detail.tsx")),
            StringComparison.Ordinal);
        Assert.Contains(
            "}, [locale, pageNumber, reloadToken, search, statusFilter])",
            File.ReadAllText(Path.Combine(frontendRoot, "admin", "admin-catalog-workspace.tsx")),
            StringComparison.Ordinal);
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
