param(
    [string]$VatSysRoot = 'C:\Program Files (x86)\vatSys'
)

$ErrorActionPreference = 'Stop'
$source = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not (Test-Path -LiteralPath (Join-Path $VatSysRoot 'bin\vatSys.exe'))) {
    throw "vatSys was not found at $VatSysRoot\bin\vatSys.exe. Pass -VatSysRoot with the folder that contains bin\vatSys.exe."
}
$target = Join-Path $VatSysRoot 'bin\Plugins\DiscordRichPresence'
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $source 'VatSys.DiscordRichPresence.dll') -Destination $target -Force
Copy-Item -LiteralPath (Join-Path $source 'presence.json') -Destination $target -Force
Copy-Item -LiteralPath (Join-Path $source 'app.ico') -Destination $target -Force
Get-ChildItem -LiteralPath $target | Select-Object Name, Length
