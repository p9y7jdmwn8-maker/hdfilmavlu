$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$dotnet = Join-Path $root ".dotnet\dotnet.exe"
if (!(Test-Path $dotnet)) {
    $dotnet = "dotnet"
}

$publishDir = Join-Path $root "deploy-output\hdfilmavlu"
$zipPath = Join-Path $root "deploy-output\hdfilmavlu-release.zip"

New-Item -ItemType Directory -Force -Path (Split-Path $publishDir -Parent) | Out-Null
if (Test-Path $publishDir) {
    Remove-Item -LiteralPath $publishDir -Recurse -Force
}

& $dotnet publish (Join-Path $root "VideoSite.csproj") -c Release -o $publishDir

New-Item -ItemType Directory -Force -Path (Join-Path $publishDir "App_Data") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $publishDir "wwwroot\uploads\videos") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $publishDir "wwwroot\uploads\posters") | Out-Null

if (Test-Path $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath

Write-Host "Publish folder: $publishDir"
Write-Host "Zip package: $zipPath"
