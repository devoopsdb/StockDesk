@echo off
setlocal
rem ============================================================================
rem StockDesk Automated Release Script (Hybrid Batch / PowerShell)
rem ============================================================================
set "SCRIPT_PATH=%~f0"
set "SCRIPT_ARGS=%*"
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "$scriptPath = $env:SCRIPT_PATH; $argsRaw = $env:SCRIPT_ARGS; & { $lines = [System.IO.File]::ReadAllLines($scriptPath); $idx = [array]::IndexOf($lines, '#POWERSHELL_START'); if ($idx -ge 0) { $code = ($lines[($idx+1)..($lines.Length-1)] -join [Environment]::NewLine); [string[]]$parsedArgs = if ($argsRaw.Trim()) { $argsRaw.Trim() -split '\s+' } else { @() }; & ([ScriptBlock]::Create($code)) @parsedArgs } else { Write-Host 'Marker #POWERSHELL_START not found' -ForegroundColor Red; exit 1 } }"
exit /b %ERRORLEVEL%

#POWERSHELL_START
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

$TargetVersionInput = $null
$DryRun = $false
$SkipTests = $false
$Help = $false
$AutoConfirm = $false

foreach ($arg in $args) {
    switch -Regex ($arg) {
        '^(?i)[/-]?-?(help|h|\?)$' { $Help = $true }
        '^(?i)[/-]?-?(dry-?run|d)$' { $DryRun = $true }
        '^(?i)[/-]?-?(skip-?tests|s)$' { $SkipTests = $true }
        '^(?i)[/-]?-?(yes|y)$' { $AutoConfirm = $true }
        default {
            if (-not $TargetVersionInput) {
                $TargetVersionInput = $arg
            }
        }
    }
}

function Show-Banner {
    Write-Host ""
    Write-Host "==========================================================" -ForegroundColor Cyan
    Write-Host "             StockDesk Release Automation Tool            " -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Show-Usage {
    Write-Host "Usage:" -ForegroundColor Yellow
    Write-Host "  update.bat                    Interactive release prompt"
    Write-Host "  update.bat <version>          Release specific version (e.g. 1.1.0)"
    Write-Host "  update.bat patch|minor|major  Auto-bump version"
    Write-Host "  update.bat -DryRun            Simulate process without modifying files"
    Write-Host "  update.bat -SkipTests         Skip running 'dotnet test'"
    Write-Host "  update.bat -Yes               Auto-confirm prompts (non-interactive)"
    Write-Host "  update.bat -Help              Show this help message"
    Write-Host ""
}

function Fail([string]$Message) {
    Write-Host ""
    Write-Host "ERROR: $Message" -ForegroundColor Red
    Write-Host ""
    exit 1
}

function Confirm-Prompt([string]$PromptText, [bool]$DefaultYes = $false) {
    if ($DryRun) {
        Write-Host "  [DryRun] Prompt bypassed: $PromptText (Simulated YES)" -ForegroundColor DarkGray
        return $true
    }
    if ($AutoConfirm) {
        Write-Host "  [AutoConfirm] $PromptText -> YES" -ForegroundColor DarkGray
        return $true
    }
    $resp = Read-Host "  $PromptText"
    if (-not $resp) {
        return $DefaultYes
    }
    return ($resp -match "^[Yy]$")
}

if ($Help -or $TargetVersionInput -in @("-h", "--help", "/?", "-?")) {
    Show-Banner
    Show-Usage
    exit 0
}

Show-Banner

# Determine repo root directory from script location
$scriptDir = Split-Path -Parent $scriptPath
if (-not $scriptDir) { $scriptDir = (Get-Location).Path }

$csprojPath = Join-Path $scriptDir "src\StockDesk\StockDesk.csproj"
$changelogPath = Join-Path $scriptDir "CHANGELOG.md"
$slnPath = Join-Path $scriptDir "StockDesk.sln"

if (-not (Test-Path -LiteralPath $csprojPath)) {
    Fail "Could not find project file at: $csprojPath"
}

if (-not (Test-Path -LiteralPath $changelogPath)) {
    Fail "Could not find CHANGELOG.md at: $changelogPath"
}

# -------------------------------------------------------------
# 1. Pre-flight Git Checks
# -------------------------------------------------------------
Write-Host "[1/6] Running pre-flight Git checks..." -ForegroundColor Yellow

$currentBranch = (git rev-parse --abbrev-ref HEAD 2>$null).Trim()
if (-not $currentBranch) {
    Fail "Not a valid Git repository or Git is not installed in PATH."
}

if ($currentBranch -ne "main") {
    Write-Host "  [WARNING] Current branch is '$currentBranch', not 'main'." -ForegroundColor Yellow
    if (-not (Confirm-Prompt "Do you want to proceed with release on '$currentBranch'? [y/N]" $false)) {
        Write-Host "Release aborted by developer." -ForegroundColor Red
        exit 0
    }
} else {
    Write-Host "  [OK] Verified active branch: $currentBranch" -ForegroundColor Green
}

# Check for uncommitted changes
$gitStatus = git status --porcelain 2>$null
if ($gitStatus) {
    Write-Host "  [WARNING] Working tree has uncommitted modifications:" -ForegroundColor Yellow
    $gitStatus | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    Write-Host "  Note: Only StockDesk.csproj and CHANGELOG.md will be staged for release." -ForegroundColor Cyan
    if (-not (Confirm-Prompt "Do you want to continue? [y/N]" $false)) {
        Write-Host "Release aborted by developer." -ForegroundColor Red
        exit 0
    }
} else {
    Write-Host "  [OK] Working tree is clean" -ForegroundColor Green
}

# -------------------------------------------------------------
# 2. Version Resolution & Collision Detection
# -------------------------------------------------------------
Write-Host "`n[2/6] Reading current version and Git tags..." -ForegroundColor Yellow

$csprojContent = [System.IO.File]::ReadAllText($csprojPath, [System.Text.Encoding]::UTF8)
if ($csprojContent -match '<Version>(?<version>.*?)</Version>') {
    $currentVersion = $Matches['version'].Trim()
} else {
    Fail "Unable to locate <Version> element in $csprojPath"
}

Write-Host "  Current project version: " -NoNewline
Write-Host $currentVersion -ForegroundColor Cyan

# Parse SemVer
if ($currentVersion -notmatch '^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)$') {
    Fail "Current version '$currentVersion' is not standard SemVer (X.Y.Z)."
}

$major = [int]$Matches['major']
$minor = [int]$Matches['minor']
$patch = [int]$Matches['patch']

# Fetch existing tags (local and remote)
Write-Host "  Fetching existing Git tags..." -ForegroundColor DarkGray
$localTags = @(git tag -l 2>$null)

$remoteTags = @()
$remoteOutput = git ls-remote --tags origin 2>$null
if ($remoteOutput) {
    foreach ($line in $remoteOutput) {
        if ($line -match 'refs/tags/(?<tag>v?[^\^]+)') {
            $remoteTags += $Matches['tag']
        }
    }
}

$allTags = ($localTags + $remoteTags) | Select-Object -Unique

function Test-TagExists([string]$ver) {
    $tagWithV = "v$ver"
    $tagNoV = "$ver"
    return ($allTags -contains $tagWithV -or $allTags -contains $tagNoV)
}

# Calculate candidate versions ensuring tag is free
$candPatchNum = $patch + 1
while (Test-TagExists "$major.$minor.$candPatchNum") {
    $candPatchNum++
}
$candPatch = "$major.$minor.$candPatchNum"

$candMinorNum = $minor + 1
$candMinor = "$major.$candMinorNum.0"
while (Test-TagExists $candMinor) {
    $candMinorNum++
    $candMinor = "$major.$candMinorNum.0"
}

$candMajorNum = $major + 1
$candMajor = "$candMajorNum.0.0"
while (Test-TagExists $candMajor) {
    $candMajorNum++
    $candMajor = "$candMajorNum.0.0"
}

$targetVersion = ""

if ($TargetVersionInput) {
    switch ($TargetVersionInput.ToLowerInvariant()) {
        "patch" { $targetVersion = $candPatch }
        "minor" { $targetVersion = $candMinor }
        "major" { $targetVersion = $candMajor }
        default {
            # Strip leading 'v' if entered by mistake
            $cleanVer = $TargetVersionInput.TrimStart('v', 'V')
            if ($cleanVer -notmatch '^\d+\.\d+\.\d+$') {
                Fail "Invalid version format '$TargetVersionInput'. Expected SemVer format (e.g. 1.1.0)."
            }
            if (Test-TagExists $cleanVer) {
                Fail "Tag 'v$cleanVer' already exists in local or remote repository! Please select an unused version."
            }
            $targetVersion = $cleanVer
        }
    }
} else {
    # Interactive Menu
    Write-Host ""
    Write-Host "Select release version:" -ForegroundColor White
    Write-Host "  [1] Patch : $candPatch" -ForegroundColor Green
    Write-Host "  [2] Minor : $candMinor (recommended for new features)" -ForegroundColor Green
    Write-Host "  [3] Major : $candMajor" -ForegroundColor Green
    Write-Host "  [4] Custom version" -ForegroundColor Yellow
    Write-Host "  [Q] Abort" -ForegroundColor DarkGray
    Write-Host ""

    while (-not $targetVersion) {
        $choice = (Read-Host "Enter choice [1-4, Q]").Trim()
        switch ($choice) {
            "1" { $targetVersion = $candPatch }
            "2" { $targetVersion = $candMinor }
            "3" { $targetVersion = $candMajor }
            "4" {
                $customVer = (Read-Host "Enter custom version (e.g. 1.2.0)").Trim().TrimStart('v', 'V')
                if ($customVer -notmatch '^\d+\.\d+\.\d+$') {
                    Write-Host "Invalid format. Must be X.Y.Z." -ForegroundColor Red
                    continue
                }
                if (Test-TagExists $customVer) {
                    Write-Host "Tag 'v$customVer' already exists! Choose another." -ForegroundColor Red
                    continue
                }
                $targetVersion = $customVer
            }
            "Q" {
                Write-Host "Release aborted." -ForegroundColor Yellow
                exit 0
            }
            "q" {
                Write-Host "Release aborted." -ForegroundColor Yellow
                exit 0
            }
            default {
                Write-Host "Invalid option. Please enter 1, 2, 3, 4, or Q." -ForegroundColor Red
            }
        }
    }
}

Write-Host "`nTarget Release Version: " -NoNewline
Write-Host $targetVersion -ForegroundColor Green
Write-Host "Target Git Tag: " -NoNewline
Write-Host "v$targetVersion" -ForegroundColor Green

# -------------------------------------------------------------
# 3. Unit Test Verification
# -------------------------------------------------------------
if (-not $SkipTests) {
    Write-Host "`n[3/6] Running automated tests..." -ForegroundColor Yellow
    Write-Host "  dotnet test StockDesk.sln -c Release --verbosity normal" -ForegroundColor DarkGray
    if (-not $DryRun) {
        & dotnet test $slnPath -c Release --verbosity normal
        if ($LASTEXITCODE -ne 0) {
            Fail "Automated unit tests failed! Aborting release."
        }
        Write-Host "  [OK] All unit tests passed successfully!" -ForegroundColor Green
    } else {
        Write-Host "  [DryRun] Skipping test execution during simulation." -ForegroundColor DarkGray
    }
} else {
    Write-Host "`n[3/6] Skipping automated tests (-SkipTests specified)." -ForegroundColor DarkGray
}

# -------------------------------------------------------------
# 4. Update StockDesk.csproj
# -------------------------------------------------------------
Write-Host "`n[4/6] Updating project version in StockDesk.csproj..." -ForegroundColor Yellow

$newCsprojContent = [System.Text.RegularExpressions.Regex]::Replace(
    $csprojContent,
    '<Version>.*?</Version>',
    "<Version>$targetVersion</Version>",
    [System.Text.RegularExpressions.RegexOptions]::Singleline
)

if ($newCsprojContent -eq $csprojContent) {
    Fail "Failed to replace <Version> tag in $csprojPath"
}

if (-not $DryRun) {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($csprojPath, $newCsprojContent, $utf8NoBom)
    Write-Host "  [OK] Updated $csprojPath -> <Version>$targetVersion</Version>" -ForegroundColor Green
} else {
    Write-Host "  [DryRun] Would update $csprojPath -> <Version>$targetVersion</Version>" -ForegroundColor DarkGray
}

# -------------------------------------------------------------
# 5. Update CHANGELOG.md
# -------------------------------------------------------------
Write-Host "`n[5/6] Updating CHANGELOG.md..." -ForegroundColor Yellow

$changelogRaw = [System.IO.File]::ReadAllText($changelogPath, [System.Text.Encoding]::UTF8)
$today = (Get-Date).ToString("yyyy-MM-dd")

# Match ## [Unreleased] up to the next release header (## [...]) or end of file
$pattern = '(?s)##\s*\[Unreleased\]\s*\r?\n(?<content>.*?)(?=(?:\r?\n##\s*\[)|$)'
$match = [System.Text.RegularExpressions.Regex]::Match($changelogRaw, $pattern)

if (-not $match.Success) {
    Fail "Could not find '## [Unreleased]' section in $changelogPath"
}

$unreleasedNotes = $match.Groups['content'].Value.Trim()
if (-not $unreleasedNotes) {
    Write-Host "  [WARNING] '## [Unreleased]' section in CHANGELOG.md appears to be empty!" -ForegroundColor Yellow
    if (-not (Confirm-Prompt "Continue without release notes? [y/N]" $false)) {
        Write-Host "Release aborted. Please document changes in CHANGELOG.md first." -ForegroundColor Red
        exit 0
    }
}

$emDash = [char]0x2014
$replacementHeader = "## [Unreleased]`r`n`r`n## [$targetVersion] $emDash $today"
if ($unreleasedNotes) {
    $replacementHeader += "`r`n`r`n" + $unreleasedNotes
}
$replacementHeader += "`r`n"

$newChangelogRaw = $changelogRaw.Substring(0, $match.Index) + $replacementHeader + $changelogRaw.Substring($match.Index + $match.Length)

if (-not $DryRun) {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($changelogPath, $newChangelogRaw, $utf8NoBom)
    Write-Host "  [OK] Moved Unreleased notes to '## [$targetVersion] $emDash $today' (UTF-8 safe)" -ForegroundColor Green
} else {
    Write-Host "  [DryRun] Would move Unreleased notes to '## [$targetVersion] $emDash $today'" -ForegroundColor DarkGray
}

# -------------------------------------------------------------
# 6. Git Commit, Tag & Push
# -------------------------------------------------------------
Write-Host "`n[6/6] Git staging, commit, tag and push..." -ForegroundColor Yellow

if ($DryRun) {
    Write-Host "  [DryRun] Would execute:" -ForegroundColor DarkGray
    Write-Host "    git add `"$csprojPath`" `"$changelogPath`"" -ForegroundColor DarkGray
    Write-Host "    git commit -m `"chore(release): bump version to $targetVersion`"" -ForegroundColor DarkGray
    Write-Host "    git tag -a `"v$targetVersion`" -m `"Release v$targetVersion`"" -ForegroundColor DarkGray
    Write-Host "    git push origin $currentBranch" -ForegroundColor DarkGray
    Write-Host "    git push origin `"v$targetVersion`"" -ForegroundColor DarkGray
    Write-Host "`n[DryRun Complete] Simulation finished successfully with zero changes made." -ForegroundColor Cyan
    exit 0
}

# Stage files
git add "$csprojPath" "$changelogPath"
Write-Host "  [OK] Staged StockDesk.csproj and CHANGELOG.md" -ForegroundColor Green

# Commit
$commitMsg = "chore(release): bump version to $targetVersion"
git commit -m $commitMsg
if ($LASTEXITCODE -ne 0) {
    Fail "Git commit failed!"
}
Write-Host "  [OK] Committed: $commitMsg" -ForegroundColor Green

# Tag
$tagName = "v$targetVersion"
git tag -a $tagName -m "Release $tagName"
if ($LASTEXITCODE -ne 0) {
    Fail "Git tag creation failed!"
}
Write-Host "  [OK] Created annotated Git tag: $tagName" -ForegroundColor Green

# Push Confirmation
Write-Host ""
Write-Host "Local release commit and tag '$tagName' are ready." -ForegroundColor Cyan
if (-not (Confirm-Prompt "Push branch '$currentBranch' and tag '$tagName' to origin to trigger GitHub Actions CI/CD? [Y/n]" $true)) {
    Write-Host "`nPush skipped. Local commit and tag are retained." -ForegroundColor Yellow
    Write-Host "When you are ready to publish, run:" -ForegroundColor White
    Write-Host "  git push origin $currentBranch" -ForegroundColor Gray
    Write-Host "  git push origin $tagName" -ForegroundColor Gray
    exit 0
}

Write-Host "Pushing to remote origin..." -ForegroundColor Yellow
git push origin $currentBranch
if ($LASTEXITCODE -ne 0) {
    Fail "Failed to push branch '$currentBranch' to origin."
}

git push origin $tagName
if ($LASTEXITCODE -ne 0) {
    Fail "Failed to push tag '$tagName' to origin."
}

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "  Release $tagName pushed successfully!                    " -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Write-Host "GitHub Actions pipeline is now running:" -ForegroundColor White
Write-Host "  1. Compiling and running tests" -ForegroundColor Gray
Write-Host "  2. Generating Velopack installer and release package" -ForegroundColor Gray
Write-Host "  3. Publishing GitHub Release" -ForegroundColor Gray
Write-Host "Active clients will automatically receive the update on next launch." -ForegroundColor Cyan
Write-Host ""