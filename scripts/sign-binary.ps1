<#
.SYNOPSIS
    Signs one or more Windows executables/DLLs/installers using Authenticode SHA256,
    supporting Base64 PFX environment secrets with an automated self-signed fallback.

.PARAMETER FilePaths
    One or more file paths to sign.

.PARAMETER CertBase64
    Base64-encoded PFX certificate string. Defaults to $env:CODE_SIGN_CERT_BASE64.

.PARAMETER CertPassword
    Password for the PFX certificate. Defaults to $env:CODE_SIGN_PASSWORD or "StockDesk2026!".

.PARAMETER CertPath
    Direct path to a .pfx file (optional).

.PARAMETER TimestampServer
    RFC 3161 Timestamp server URL (default: "http://timestamp.digicert.com").

.PARAMETER Description
    Description embedded in the signature (default: "StockDesk").
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [string[]]$FilePaths,

    [string]$CertBase64 = $env:CODE_SIGN_CERT_BASE64,
    [string]$CertPassword = $env:CODE_SIGN_PASSWORD,
    [string]$CertPath = "",
    [string]$TimestampServer = "http://timestamp.digicert.com",
    [string]$Description = "StockDesk"
)

$ErrorActionPreference = "Stop"

if (-not $CertPassword) {
    $CertPassword = "StockDesk2026!"
}

# 1. Locate SignTool.exe
function Find-SignTool {
    # Check PATH first
    $cmd = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    # Check Windows SDK directories
    $sdkRoots = @(
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
        "${env:ProgramFiles}\Windows Kits\10\bin"
    )

    foreach ($root in $sdkRoots) {
        if (Test-Path $root) {
            $tools = Get-ChildItem -Path $root -Filter "signtool.exe" -Recurse -ErrorAction SilentlyContinue |
                     Where-Object { $_.FullName -match "x64" } |
                     Sort-Object LastWriteTime -Descending
            if ($tools -and $tools.Count -gt 0) {
                return $tools[0].FullName
            }
        }
    }
    return $null
}

$signtool = Find-SignTool
if ($signtool) {
    Write-Host "Found SignTool: $signtool" -ForegroundColor Gray
} else {
    Write-Host "SignTool.exe not found in standard SDK paths. Will use Set-AuthenticodeSignature fallback." -ForegroundColor Yellow
}

$tempPfxCreated = $false
$actualPfxPath = $CertPath

try {
    # 2. Determine or generate certificate PFX
    if ($CertBase64) {
        Write-Host "Decoding Code Signing Certificate from Base64 secret..." -ForegroundColor Cyan
        $tempPfxPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "stockdesk_sign_$([System.Guid]::NewGuid().ToString('N')).pfx")
        [System.IO.File]::WriteAllBytes($tempPfxPath, [Convert]::FromBase64String($CertBase64))
        $actualPfxPath = $tempPfxPath
        $tempPfxCreated = $true
    }
    elseif (-not $actualPfxPath -or -not (Test-Path $actualPfxPath)) {
        Write-Host "No certificate secret provided. Generating automated self-signed fallback certificate..." -ForegroundColor Yellow
        $tempPfxPath = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "stockdesk_fallback_$([System.Guid]::NewGuid().ToString('N')).pfx")
        $tempCerPath = [System.IO.Path]::ChangeExtension($tempPfxPath, ".cer")
        
        $genScript = Join-Path $PSScriptRoot "generate-cert.ps1"
        if (Test-Path $genScript) {
            & $genScript -Password $CertPassword -PfxPath $tempPfxPath -CerPath $tempCerPath | Out-Null
        } else {
            $notBefore = [DateTime]::UtcNow.AddDays(-1)
            $notAfter = [DateTime]::UtcNow.AddYears(1)
            $cert = New-SelfSignedCertificate `
                -Subject "CN=StockDesk Fallback Code Signing" `
                -Type CodeSigningCert `
                -KeyAlgorithm RSA `
                -KeyLength 2048 `
                -NotBefore $notBefore `
                -NotAfter $notAfter `
                -CertStoreLocation "Cert:\CurrentUser\My"
            $secPassword = ConvertTo-SecureString -String $CertPassword -AsPlainText -Force
            Export-PfxCertificate -Cert $cert -FilePath $tempPfxPath -Password $secPassword | Out-Null
            Remove-Item -Path "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force -ErrorAction SilentlyContinue
        }

        $actualPfxPath = $tempPfxPath
        $tempPfxCreated = $true
    }

    # 3. Sign each file
    foreach ($file in $FilePaths) {
        if (-not (Test-Path $file)) {
            Write-Warning "File not found for signing: $file"
            continue
        }

        $resolvedFile = (Resolve-Path $file).Path
        Write-Host "Signing: $resolvedFile" -ForegroundColor Cyan

        if ($signtool) {
            & $signtool sign /f $actualPfxPath /p $CertPassword /fd SHA256 /tr $TimestampServer /td SHA256 /d $Description $resolvedFile
            if ($LASTEXITCODE -ne 0) {
                Write-Warning "SignTool failed with exit code $LASTEXITCODE. Retrying without timestamp..."
                & $signtool sign /f $actualPfxPath /p $CertPassword /fd SHA256 /d $Description $resolvedFile
                if ($LASTEXITCODE -ne 0) {
                    throw "SignTool failed to sign $resolvedFile (Exit code: $LASTEXITCODE)"
                }
            }
        } else {
            # Fallback to Set-AuthenticodeSignature
            $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2(
                $actualPfxPath,
                $CertPassword,
                [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable
            )
            Set-AuthenticodeSignature -FilePath $resolvedFile -Certificate $cert -HashAlgorithm SHA256 -TimestampServer $TimestampServer | Out-Null
        }

        # Verify signature
        $sig = Get-AuthenticodeSignature -FilePath $resolvedFile
        Write-Host "Verified signature status for $($sig.Path): $($sig.Status) (Signer: $($sig.SignerCertificate.Subject))" -ForegroundColor Green
    }
}
finally {
    # 4. Secure Cleanup
    if ($tempPfxCreated -and (Test-Path $actualPfxPath)) {
        Remove-Item -Path $actualPfxPath -Force -ErrorAction SilentlyContinue
        Write-Host "Safely removed temporary PFX key from runner." -ForegroundColor Gray
    }
}
