using System.Text.RegularExpressions;

namespace ECommerce.ContractTests;

public sealed partial class PaymentCardDataBoundaryTests
{
    [Fact]
    public void BackendSourceDoesNotDeclareOrPersistRawPaymentCardFields()
    {
        string root = FindRepositoryRoot();
        string sourceRoot = Path.Combine(root, "src");
        string redactionFile = Path.GetFullPath(Path.Combine(
            sourceRoot,
            "BuildingBlocks",
            "ECommerce.BuildingBlocks.Observability",
            "SensitiveDataRedaction.cs"));
        List<string> violations = [];

        foreach (string path in Directory.EnumerateFiles(
                     sourceRoot,
                     "*.cs",
                     SearchOption.AllDirectories))
        {
            string fullPath = Path.GetFullPath(path);
            if (fullPath.Contains(
                    $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(
                    $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fullPath, redactionFile, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int lineNumber = 0;
            foreach (string line in File.ReadLines(fullPath))
            {
                lineNumber++;
                Match match = RawPaymentCardIdentifier().Match(line);
                if (match.Success)
                {
                    violations.Add(
                        $"{Path.GetRelativePath(root, fullPath)}:{lineNumber} ({match.Value})");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Raw payment-card identifiers are forbidden in backend API, event, log, domain, and persistence source:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void FrontendSourceDoesNotDeclareRawPaymentCardFormFields()
    {
        string root = FindRepositoryRoot();
        string frontendRoot = Path.Combine(root, "src", "Frontend");
        List<string> violations = [];
        IEnumerable<string> paths = Directory
            .EnumerateFiles(frontendRoot, "*.ts", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(
                frontendRoot,
                "*.tsx",
                SearchOption.AllDirectories));

        foreach (string path in paths)
        {
            string fullPath = Path.GetFullPath(path);
            if (fullPath.Contains(
                    $"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase) ||
                fullPath.Contains(
                    $"{Path.DirectorySeparatorChar}.next{Path.DirectorySeparatorChar}",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int lineNumber = 0;
            foreach (string line in File.ReadLines(fullPath))
            {
                lineNumber++;
                Match match = RawPaymentCardIdentifier().Match(line);
                if (match.Success)
                {
                    violations.Add(
                        $"{Path.GetRelativePath(root, fullPath)}:{lineNumber} ({match.Value})");
                }
            }
        }

        Assert.True(
            violations.Count == 0,
            "Raw payment-card form identifiers are forbidden while DemoPaymentProvider is active:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "ECommerce.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException(
                "Repository root containing ECommerce.sln was not found.");
    }

    [GeneratedRegex(
        @"\b(?:card_?number|credit_?card_?number|primary_?account_?number|pan|cvv|cvc|card_?security_?code|security_?code|card_?holder(?:_?name)?|card_?expiry|card_?expiration|expiry_?(?:month|year)|expiration_?(?:month|year))\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex RawPaymentCardIdentifier();
}
