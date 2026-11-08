param(
    [Parameter(Mandatory = $true)][string]$BuildDirectory,
    [Parameter(Mandatory = $true)][string]$ReadmePath,
    [Parameter(Mandatory = $true)][string]$DeclarationPath,
    [Parameter(Mandatory = $true)][string]$ZipPath
)

$ErrorActionPreference = 'Stop'
$build = (Resolve-Path -LiteralPath $BuildDirectory).Path
$indexPath = Join-Path $build 'index.html'
$cssPath = Join-Path $build 'TemplateData\style.css'
if (-not (Test-Path -LiteralPath $indexPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $cssPath -PathType Leaf)) {
    throw "Unity WebGL output is incomplete: $build"
}

$index = [IO.File]::ReadAllText($indexPath)
$index = [regex]::Replace($index, '<title>[^<]*</title>', '<title>The Winding Journey</title>')
$index = [regex]::Replace($index, '<canvas id="unity-canvas" width="?\d+"? height="?\d+"?', '<canvas id="unity-canvas" width="960" height="540"')
$index = [regex]::Replace($index, 'canvas\.style\.width = "\d+px";', 'canvas.style.width = "100%";')
$index = [regex]::Replace($index, 'canvas\.style\.height = "\d+px";', 'canvas.style.height = "100%";')
if ($index -notmatch '<title>The Winding Journey</title>' -or
    $index -notmatch 'canvas\.style\.width = "100%";' -or
    $index -notmatch 'canvas\.style\.height = "100%";') {
    throw 'The Unity HTML template changed; responsive sizing was not applied.'
}
[IO.File]::WriteAllText($indexPath, $index, [Text.UTF8Encoding]::new($false))

$css = [IO.File]::ReadAllText($cssPath)
$responsiveCss = @'

/* Fill itch.io's embedded frame and keep the game at its chosen 16:9 viewport. */
html, body { width: 100%; height: 100%; overflow: hidden; }
#unity-container.unity-desktop { left: 0; top: 0; transform: none; width: 100vw; height: 100vh; }
#unity-canvas { display: block; width: 100% !important; height: 100% !important; }
#unity-footer { display: none; }
'@
if (-not $css.Contains("Fill itch.io's embedded frame")) {
    [IO.File]::WriteAllText($cssPath, $css + $responsiveCss, [Text.UTF8Encoding]::new($false))
}

Copy-Item -LiteralPath $ReadmePath -Destination (Join-Path $build 'README.md') -Force
Copy-Item -LiteralPath $DeclarationPath -Destination (Join-Path $build 'Asset_Usage_Declaration.md') -Force
$licenseSource = Join-Path $PSScriptRoot '..\ThirdPartyLicenses\Chewy-LICENSE.txt'
if (-not (Test-Path -LiteralPath $licenseSource -PathType Leaf)) {
    throw "Missing Chewy font license: $licenseSource"
}
$licenseFolder = Join-Path $build 'ThirdPartyLicenses'
New-Item -ItemType Directory -Path $licenseFolder -Force | Out-Null
Copy-Item -LiteralPath $licenseSource -Destination (Join-Path $licenseFolder 'Chewy-LICENSE.txt') -Force
Compress-Archive -Path (Join-Path $build '*') -DestinationPath $ZipPath -CompressionLevel Optimal -Force
Write-Output "Packaged $ZipPath"
