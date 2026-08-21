[CmdletBinding()]
param(
    [string]$Environment = "staging",
    [string]$GatewayBaseUrl = "http://localhost:15080",
    [Guid]$ProductId = [Guid]::Empty
)

$ErrorActionPreference = "Stop"

$runner = Join-Path $PSScriptRoot "run-with-secrets.ps1"
$probe = Join-Path $PSScriptRoot "check-seller-media.ps1"
$secureToken = $null
$tokenPointer = [IntPtr]::Zero

try {
    $secureToken = Read-Host `
        "Bearer olmadan kisa omurlu Seller access tokenini yapistir" `
        -AsSecureString
    $tokenPointer =
        [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secureToken)
    $env:SellerChecks__AccessToken =
        [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenPointer)

    $probeArguments = @(
        $probe,
        "-GatewayBaseUrl",
        $GatewayBaseUrl
    )
    if ($ProductId -ne [Guid]::Empty) {
        $probeArguments += @("-ProductId", $ProductId.ToString())
    }

    & $runner -Environment $Environment -Command $probeArguments
}
finally {
    Remove-Item "Env:\SellerChecks__AccessToken" -ErrorAction SilentlyContinue

    if ($tokenPointer -ne [IntPtr]::Zero) {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenPointer)
    }

    $secureToken = $null
    $tokenPointer = [IntPtr]::Zero
    Set-Clipboard -Value " " -ErrorAction SilentlyContinue
}
