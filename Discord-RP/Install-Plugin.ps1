param(
    [string]$VatSysRoot = 'C:\Program Files (x86)\vatSys'
)

$ErrorActionPreference = 'Stop'
$source = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (Test-Path -LiteralPath (Join-Path $VatSysRoot 'bin\vatSys.exe'))) {
    throw "vatSys was not found at $VatSysRoot\bin\vatSys.exe. Pass -VatSysRoot with the folder that contains bin\vatSys.exe."
}
# Versions before the rename installed VatSys.DiscordRichPresence.dll here; leaving it would load two presence plugins.
$legacy = Join-Path $VatSysRoot 'bin\Plugins\DiscordRichPresence'
if (Test-Path -LiteralPath $legacy) {
    Remove-Item -LiteralPath $legacy -Recurse -Force
    Write-Output "Removed old plugin folder: $legacy"
}
$target = Join-Path $VatSysRoot 'bin\Plugins\Discord-RP'
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $source 'Discord-RP.dll') -Destination $target -Force
Copy-Item -LiteralPath (Join-Path $source 'presence.json') -Destination $target -Force
Copy-Item -LiteralPath (Join-Path $source 'app.ico') -Destination $target -Force
Get-ChildItem -LiteralPath $target | Select-Object Name, Length
