using System.Text.RegularExpressions;

namespace ECommerce.ContractTests;

public sealed class PersistenceAndCqrsArchitectureTests
{
    [Fact]
    public void ServicesDoNotDeclareSpecializedRepositoryTypes()
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");
        Regex specializedRepository = new(
            @"\b(?:interface\s+I\w+Repository|class\s+\w+Repository)\b",
            RegexOptions.CultureInvariant);

        string[] violations = Directory
            .EnumerateFiles(servicesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => specializedRepository.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(servicesRoot, path))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void TransportEntryPointsDependOnCqrsHandlersInsteadOfApplicationServices()
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");
        Regex directServiceDependency = new(
            @"\b\w+Service\s+\w+",
            RegexOptions.CultureInvariant);

        string[] violations = Directory
            .EnumerateFiles(servicesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path =>
                path.EndsWith("Endpoints.cs", StringComparison.Ordinal) ||
                path.EndsWith("Consumer.cs", StringComparison.Ordinal))
            .Where(path => directServiceDependency.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(servicesRoot, path))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void TransportEntryPointsDoNotDependOnPersistenceOrReaderPorts()
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");
        Regex directPersistenceDependency = new(
            @"\b(?:IRepository\s*<|IUnitOfWork\b|I\w+(?:Reader|Repository)\b|DbContext\b|DbSet\s*<)",
            RegexOptions.CultureInvariant);

        string[] violations = EnumerateTransportEntryPoints(servicesRoot)
            .Where(path => directPersistenceDependency.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(servicesRoot, path))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ApplicationProjectsDoNotDependOnEntityFrameworkCoreOrDbContext()
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");
        Regex persistenceDependency = new(
            @"\b(?:Microsoft\.EntityFrameworkCore|DbContext|DbSet\s*<|IQueryable\s*<)\b",
            RegexOptions.CultureInvariant);

        string[] violations = EnumerateApplicationFiles(servicesRoot)
            .Where(path => persistenceDependency.IsMatch(File.ReadAllText(path)))
            .Select(path => Path.GetRelativePath(servicesRoot, path))
            .ToArray();

        Assert.Empty(violations);
    }

    [Theory]
    [InlineData("ICommandHandler<", "Commands")]
    [InlineData("IQueryHandler<", "Queries")]
    public void CqrsHandlersStayInMatchingFeatureFolders(
        string handlerContract,
        string expectedFolder)
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");

        string[] violations = EnumerateApplicationFiles(servicesRoot)
            .Where(path => path.EndsWith("Handler.cs", StringComparison.Ordinal))
            .Where(path => File.ReadAllText(path).Contains(handlerContract, StringComparison.Ordinal))
            .Where(path =>
            {
                string[] segments = Path.GetRelativePath(servicesRoot, path)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string source = File.ReadAllText(path);
                bool namespaceMatches = Regex.IsMatch(
                    source,
                    $@"\bnamespace\s+[\w.]+\.{expectedFolder}(?:\.|;)",
                    RegexOptions.CultureInvariant);
                return !segments.Contains(expectedFolder, StringComparer.Ordinal) ||
                    !namespaceMatches;
            })
            .Select(path => Path.GetRelativePath(servicesRoot, path))
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ServiceProjectReferencesFollowInwardLayerDirection()
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");
        Dictionary<string, string[]> forbiddenTargetsByLayer = new(StringComparer.Ordinal)
        {
            ["Domain"] = [".Application", ".Infrastructure", ".Api", ".Worker"],
            ["Application"] = [".Infrastructure", ".Api", ".Worker"],
            ["Infrastructure"] = [".Api", ".Worker"]
        };

        string[] violations = EnumerateServiceProjects(servicesRoot)
            .SelectMany(project =>
            {
                string sourceLayer = Path.GetFileNameWithoutExtension(project)
                    .Split('.')
                    .Last();
                if (!forbiddenTargetsByLayer.TryGetValue(
                    sourceLayer,
                    out string[]? forbiddenTargets))
                {
                    return [];
                }

                return EnumerateProjectReferences(project)
                    .Where(reference => forbiddenTargets.Any(forbidden =>
                        Path.GetFileNameWithoutExtension(reference)
                            .EndsWith(forbidden, StringComparison.Ordinal)))
                    .Select(reference =>
                        $"{Path.GetRelativePath(servicesRoot, project)} -> " +
                        Path.GetRelativePath(servicesRoot, reference));
            })
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ServiceProjectsDoNotReferenceAnotherServiceDirectly()
    {
        string servicesRoot = Path.Combine(FindRepositoryRoot(), "src", "Services");

        string[] violations = EnumerateServiceProjects(servicesRoot)
            .SelectMany(project =>
            {
                string sourceService = GetServiceName(servicesRoot, project);
                return EnumerateProjectReferences(project)
                    .Where(reference => Path.GetFullPath(reference)
                        .StartsWith(
                            Path.GetFullPath(servicesRoot) + Path.DirectorySeparatorChar,
                            StringComparison.OrdinalIgnoreCase))
                    .Where(reference =>
                        !string.Equals(
                            sourceService,
                            GetServiceName(servicesRoot, reference),
                            StringComparison.Ordinal))
                    .Select(reference =>
                        $"{Path.GetRelativePath(servicesRoot, project)} -> " +
                        Path.GetRelativePath(servicesRoot, reference));
            })
            .ToArray();

        Assert.Empty(violations);
    }

    private static IEnumerable<string> EnumerateApplicationFiles(string servicesRoot)
    {
        return Directory
            .EnumerateFiles(servicesRoot, "*.*", SearchOption.AllDirectories)
            .Where(path =>
                path.EndsWith(".cs", StringComparison.Ordinal) ||
                path.EndsWith(".csproj", StringComparison.Ordinal))
            .Where(path =>
            {
                string[] segments = Path.GetRelativePath(servicesRoot, path)
                    .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return segments.Any(segment =>
                    segment.EndsWith(".Application", StringComparison.Ordinal)) &&
                    !segments.Contains("bin", StringComparer.OrdinalIgnoreCase) &&
                    !segments.Contains("obj", StringComparer.OrdinalIgnoreCase);
            });
    }

    private static IEnumerable<string> EnumerateTransportEntryPoints(string servicesRoot)
    {
        return Directory
            .EnumerateFiles(servicesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path =>
                path.EndsWith("Endpoints.cs", StringComparison.Ordinal) ||
                path.EndsWith("Consumer.cs", StringComparison.Ordinal));
    }

    private static IEnumerable<string> EnumerateServiceProjects(string servicesRoot)
    {
        return Directory.EnumerateFiles(
            servicesRoot,
            "*.csproj",
            SearchOption.AllDirectories);
    }

    private static IEnumerable<string> EnumerateProjectReferences(string project)
    {
        Regex projectReference = new(
            @"<ProjectReference\s+Include=""([^""]+)""",
            RegexOptions.CultureInvariant);
        string projectDirectory = Path.GetDirectoryName(project)
            ?? throw new DirectoryNotFoundException($"Project directory was not found for '{project}'.");

        return projectReference
            .Matches(File.ReadAllText(project))
            .Select(match => Path.GetFullPath(Path.Combine(
                projectDirectory,
                match.Groups[1].Value)));
    }

    private static string GetServiceName(string servicesRoot, string path)
    {
        return Path.GetRelativePath(servicesRoot, path)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
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
