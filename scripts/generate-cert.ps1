<#
.SYNOPSIS
    Generates a self-signed Code Signing certificate, exports .pfx and .cer files,
    and outputs the Base64 representation suitable for GitHub Secrets.

.PARAMETER CertSubject
    The subject name for the certificate (default: "CN=StockDesk, O=StockDesk").

.PARAMETER Password
    The password used to protect the exported .pfx private key.

.PARAMETER PfxPath
    Output path for the private key container (.pfx).

.PARAMETER CerPath
    Output path for the public certificate (.cer).

.PARAMETER ValidYears
    Validity period in years (default: 5).
#>
[CmdletBinding()]
param(
    [string]$CertSubject = "CN=StockDesk Code Signing, O=StockDesk",
    [string]$Password = "StockDesk2026!",
    [string]$PfxPath = "$PSScriptRoot\StockDesk.pfx",
    [string]$CerPath = "$PSScriptRoot\StockDesk.cer",
    [int]$ValidYears = 5
)

$ErrorActionPreference = "Stop"

Write-Host "==> Generating Self-Signed Code Signing Certificate..." -ForegroundColor Cyan

$notBefore = [DateTime]::UtcNow.AddDays(-1)
$notAfter = [DateTime]::UtcNow.AddYears($ValidYears)

$cert = New-SelfSignedCertificate `
    -Subject $CertSubject `
    -Type CodeSigningCert `
    -KeyAlgorithm RSA `
    -KeyLength 4096 `
    -HashAlgorithm SHA256 `
    -NotBefore $notBefore `
    -NotAfter $notAfter `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyUsage DigitalSignature `
    -KeyExportPolicy Exportable

Write-Host "Created certificate with Thumbprint: $($cert.Thumbprint)" -ForegroundColor Green

# Ensure destination directory exists
$pfxDir = Split-Path -Parent $PfxPath
if ($pfxDir -and -not (Test-Path $pfxDir)) {
    New-Item -ItemType Directory -Path $pfxDir -Force | Out-Null
}

$secPassword = ConvertTo-SecureString -String $Password -AsPlainText -Force

# Export PFX (Private Key + Certificate)
Export-PfxCertificate -Cert $cert -FilePath $PfxPath -Password $secPassword | Out-Null
Write-Host "Exported PFX to: $PfxPath" -ForegroundColor Green

# Export CER (Public Certificate only)
Export-Certificate -Cert $cert -FilePath $CerPath | Out-Null
Write-Host "Exported CER to: $CerPath" -ForegroundColor Green

# Generate Base64 string for GitHub Secrets
$pfxBytes = [System.IO.File]::ReadAllBytes($PfxPath)
$base64Pfx = [Convert]::ToBase64String($pfxBytes)

Write-Host ""
Write-Host "================ GitHub Secrets Configuration ================" -ForegroundColor Yellow
Write-Host "1. CODE_SIGN_CERT_BASE64: (Base64 string below)" -ForegroundColor Yellow
Write-Host "2. CODE_SIGN_PASSWORD: $Password" -ForegroundColor Yellow
Write-Host "==============================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host $base64Pfx
Write-Host ""

# Clean up certificate from local store
Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force -ErrorAction SilentlyContinue
Write-Host "Cleaned up temporary cert from local Cert store." -ForegroundColor Gray
