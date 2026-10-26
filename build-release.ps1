param(
    [string]$VatSysExe = (Join-Path $PSScriptRoot '..\vatSys\bin\vatSys.exe'),
    [string]$AppIcon = (Join-Path $PSScriptRoot '..\source-code\vatSys\app.ico'),
    [string]$Version = '1.0.3'
)

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$VatSysExe = [System.IO.Path]::GetFullPath($VatSysExe)
$AppIcon = [System.IO.Path]::GetFullPath($AppIcon)
$vatSysDll = Join-Path (Split-Path -Parent $VatSysExe) 'RossCarlson.Vatsim.Network.dll'
if (-not (Test-Path -LiteralPath $VatSysExe -PathType Leaf)) { throw "vatSys executable not found: $VatSysExe" }
if (-not (Test-Path -LiteralPath $vatSysDll -PathType Leaf)) { throw "vatSys network reference not found: $vatSysDll" }
if (-not (Test-Path -LiteralPath $AppIcon -PathType Leaf)) { throw "vatSys icon not found: $AppIcon" }

$project = Join-Path $root 'DiscordRichPresence\DiscordRichPresence.csproj'
dotnet restore $project --ignore-failed-sources
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }
dotnet build $project -c Release --no-restore "-p:VatSysPath=$VatSysExe" "-p:AppIconPath=$AppIcon"
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

# The archive mirrors the vatSys installation folder, so it can be extracted
# straight into the vatSys root (the folder that contains bin\vatSys.exe).
# vatSys loads every DLL under <vatSys root>\bin\Plugins for all profiles.
$release = Join-Path $root 'release'
if (Test-Path -LiteralPath $release) { Remove-Item -LiteralPath $release -Recurse -Force }
$pluginDir = Join-Path $release 'bin\Plugins\DiscordRichPresence'
New-Item -ItemType Directory -Path $pluginDir | Out-Null
$output = Join-Path $root 'DiscordRichPresence\bin\Release\net472'
Copy-Item (Join-Path $output 'VatSys.DiscordRichPresence.dll') $pluginDir
Copy-Item (Join-Path $root 'DiscordRichPresence\presence.json') $pluginDir
Copy-Item $AppIcon (Join-Path $pluginDir 'app.ico')
Copy-Item (Join-Path $root 'LICENSE') $pluginDir

$zip = Join-Path $root "vatSys-Discord-RP-v$Version.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::Open($zip, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    Get-ChildItem -LiteralPath $release -Recurse -File | ForEach-Object {
        $entry = $_.FullName.Substring($release.Length + 1).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $_.FullName, $entry) | Out-Null
    }
}
finally { $archive.Dispose() }
Write-Output "Release package: $zip"
