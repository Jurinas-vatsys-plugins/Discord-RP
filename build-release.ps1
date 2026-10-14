$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$vatSysExe = 'I:\vatSys\vatSys\bin\vatSys.exe'
$vatSysDll = 'I:\vatSys\vatSys\bin\RossCarlson.Vatsim.Network.dll'
if (-not (Test-Path -LiteralPath $vatSysExe -PathType Leaf)) { throw "vatSys executable not found: $vatSysExe" }
if (-not (Test-Path -LiteralPath $vatSysDll -PathType Leaf)) { throw "vatSys network reference not found: $vatSysDll" }
dotnet restore (Join-Path $root 'DiscordRichPresence\DiscordRichPresence.csproj') --ignore-failed-sources
dotnet build (Join-Path $root 'DiscordRichPresence\DiscordRichPresence.csproj') -c Release --no-restore -p:VatSysPath=$vatSysExe
$release = Join-Path $root 'release'
if (Test-Path -LiteralPath $release) { Remove-Item -LiteralPath $release -Recurse -Force }
New-Item -ItemType Directory -Path $release | Out-Null
$output = Join-Path $root 'DiscordRichPresence\bin\Release\net472'
Copy-Item (Join-Path $output 'VatSys.DiscordRichPresence.dll') $release
Copy-Item (Join-Path $root 'DiscordRichPresence\presence.json') $release
Copy-Item (Join-Path $root 'VATSYS-Recovered-Source-main\vatSys\app.ico') (Join-Path $release 'app.ico')
Copy-Item (Join-Path $root 'LICENSE') (Join-Path $release 'LICENSE')
$zip = Join-Path $root 'vatSys-Discord-RP-v1.0.2.zip'
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }
Compress-Archive -Path (Join-Path $release '*') -DestinationPath $zip
Write-Output "Release package: $zip"
