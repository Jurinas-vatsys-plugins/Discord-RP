$ErrorActionPreference = 'Stop'
$source = Split-Path -Parent $MyInvocation.MyCommand.Path
$target = 'C:\Program Files (x86)\vatSys\bin\Plugins\DiscordRichPresence'
if (-not (Test-Path -LiteralPath 'C:\Program Files (x86)\vatSys\bin\vatSys.exe')) {
    throw 'Installed vatSys was not found at C:\Program Files (x86)\vatSys\bin\vatSys.exe.'
}
New-Item -ItemType Directory -Path $target -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $source 'VatSys.DiscordRichPresence.dll') -Destination $target -Force
Copy-Item -LiteralPath (Join-Path $source 'presence.json') -Destination $target -Force
Copy-Item -LiteralPath (Join-Path $source 'app.ico') -Destination $target -Force
Get-ChildItem -LiteralPath $target | Select-Object Name, Length
