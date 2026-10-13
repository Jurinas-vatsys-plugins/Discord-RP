param(
    [string]$IconPath = "$PSScriptRoot\..\VATSYS-Recovered-Source-main\vatSys\app.ico",
    [string]$OutputPath = "$PSScriptRoot\vatsys.png"
)
$ErrorActionPreference = 'Stop'
if (-not (Test-Path -LiteralPath $IconPath -PathType Leaf)) { throw "Icon not found: $IconPath" }
$parent = Split-Path -Parent ([System.IO.Path]::GetFullPath($OutputPath))
if (-not (Test-Path -LiteralPath $parent -PathType Container)) { throw "Output directory not found: $parent" }
Add-Type -AssemblyName System.Drawing
$icon = New-Object System.Drawing.Icon($IconPath, 256, 256)
$bitmap = $icon.ToBitmap()
try { $bitmap.Save($OutputPath, [System.Drawing.Imaging.ImageFormat]::Png) }
finally { $bitmap.Dispose(); $icon.Dispose() }
Write-Output "Discord asset exported: $OutputPath"
