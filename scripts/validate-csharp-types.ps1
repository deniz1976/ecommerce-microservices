param(
    [string]$Root = "."
)

$ErrorActionPreference = "Stop"

$declarationPattern = '^\s*(?:public|internal|private|protected|file)?\s*(?:(?:sealed|static|abstract|partial|readonly|ref)\s+)*(?:class|interface|enum|struct|record(?:\s+(?:class|struct))?|delegate)\s+[A-Za-z_][A-Za-z0-9_]*'
$violations = @()

$sourceFiles = Get-ChildItem -Path $Root -Recurse -File -Filter *.cs |
    Where-Object {
        $_.FullName -notmatch '[\\/](?:bin|obj)[\\/]'
    }

foreach ($sourceFile in $sourceFiles) {
    $declarations = @(Select-String -LiteralPath $sourceFile.FullName -Pattern $declarationPattern)
    if ($declarations.Count -gt 1) {
        $relativePath = [System.IO.Path]::GetRelativePath((Resolve-Path $Root), $sourceFile.FullName)
        $violations += "$relativePath declares $($declarations.Count) types at lines $($declarations.LineNumber -join ', ')"
    }
}

if ($violations.Count -gt 0) {
    $violations | ForEach-Object { Write-Error $_ }
    throw "Each C# source file must declare at most one class, record, interface, enum, struct, or delegate."
}

Write-Host "C# one-type-per-file validation completed"
