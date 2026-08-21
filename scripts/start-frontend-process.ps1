[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ApiBaseUrl
)

$ErrorActionPreference = "Stop"
$env:NEXT_PUBLIC_API_BASE_URL = $ApiBaseUrl

& npm run dev

if ($LASTEXITCODE -ne 0) {
    throw "Frontend process failed with exit code $LASTEXITCODE"
}
