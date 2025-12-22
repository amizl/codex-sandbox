Param()

# Build script for creating MSI using WiX Heat/Candle/Light
Set-StrictMode -Version Latest

$publishDir = Resolve-Path "..\bin\Release\net8.0-windows\win-x64\publish"
$installerDir = Resolve-Path .
$outDir = Join-Path $installerDir "output"
New-Item -Path $outDir -ItemType Directory -Force | Out-Null

# Locate WiX tools
$heat = Get-Command heat.exe -ErrorAction SilentlyContinue
$candle = Get-Command candle.exe -ErrorAction SilentlyContinue
$light = Get-Command light.exe -ErrorAction SilentlyContinue

if (-not $heat -or -not $candle -or -not $light) {
    Write-Error "WiX toolset (heat.exe, candle.exe, light.exe) not found. Install WiX Toolset 3.11+ and ensure tools are on PATH: https://wixtoolset.org/"
    exit 1
}

$heatPath = $heat.Path
$candlePath = $candle.Path
$lightPath = $light.Path

Write-Host "Harvesting files from $publishDir..."

# heat options:
#  -cg ProductComponents -> component group id
#  -dr INSTALLFOLDER -> directory reference id used in Product.wxs
#  -sfrag -> suppress fragment root element
#  -srd -> suppress root directory element
#  -var var.SourceDir -> create variables to make the wix file relocatable

& $heatPath dir $publishDir -cg ProductComponents -dr INSTALLFOLDER -sfrag -srd -var var.SourceDir -out "$installerDir\HarvestedFiles.wxs"

if ($LASTEXITCODE -ne 0) { Write-Error "heat failed"; exit 2 }

Write-Host "Compiling .wxs files..."

& $candlePath -dSourceDir="$publishDir" -out "$installerDir\obj\" "$installerDir\Product.wxs" "$installerDir\HarvestedFiles.wxs"
if ($LASTEXITCODE -ne 0) { Write-Error "candle failed"; exit 3 }

Write-Host "Linking to MSI..."

& $lightPath -out "$outDir\PromptEngine.Desktop.msi" "$installerDir\obj\Product.wixobj" "$installerDir\obj\HarvestedFiles.wixobj"
if ($LASTEXITCODE -ne 0) { Write-Error "light failed"; exit 4 }

Write-Host "MSI created: $outDir\PromptEngine.Desktop.msi"

# Optional: sign MSI if signtool available in PATH
$signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
if ($signtool) {
    Write-Host "Signing MSI with available certificate from store..."
    & $signtool.Path sign /a /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 "$outDir\PromptEngine.Desktop.msi"
    if ($LASTEXITCODE -eq 0) { Write-Host "MSI signed." } else { Write-Warning "Signing failed or no suitable cert found." }
}

Write-Host "Done."
