Add-Type -AssemblyName System.Drawing
$project = Split-Path -Parent $PSScriptRoot
$artRoot = Join-Path $project 'Assets\_Game\Resources\Level03'
$outDir = Join-Path $project 'Level03ArtPreview'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

function Add-Layer($graphics, $asset, [double]$x, [double]$y, [double]$opacity) {
    $path = Join-Path $artRoot ($asset + '.png')
    $image = [System.Drawing.Image]::FromFile($path)
    try {
        $left = (2048 + $x * 200 - $image.Width / 2) * .25
        $top = (857 - $y * 200 - $image.Height / 2) * .25
        $rect = [System.Drawing.RectangleF]::new(
            [float]$left, [float]$top,
            [float]($image.Width * .25), [float]($image.Height * .25))
        $attributes = [System.Drawing.Imaging.ImageAttributes]::new()
        try {
            $matrix = [System.Drawing.Imaging.ColorMatrix]::new()
            $matrix.Matrix33 = [float]$opacity
            $attributes.SetColorMatrix($matrix)
            $graphics.DrawImage($image, [System.Drawing.Rectangle]::Round($rect),
                0, 0, $image.Width, $image.Height,
                [System.Drawing.GraphicsUnit]::Pixel, $attributes)
        } finally { $attributes.Dispose() }
    } finally { $image.Dispose() }
}

foreach ($state in @('Faded', 'Reborn', 'Leaves')) {
    $canvas = [System.Drawing.Bitmap]::new(1024, 429)
    $g = [System.Drawing.Graphics]::FromImage($canvas)
    try {
        $g.Clear([System.Drawing.Color]::FromArgb(143,184,229))
        $base = if ($state -eq 'Faded') { 'Faded' } else { 'Reborn' }
        Add-Layer $g "$base/Sky" 0 0 1
        Add-Layer $g "$base/Mist" -4.00 -2.15 .08
        $leftCloudX = if ($base -eq 'Faded') { -20.25 } else { -20.19 }
        $leftCloudY = if ($base -eq 'Faded') { .14 } else { .1 }
        Add-Layer $g "$base/Cloud" $leftCloudX $leftCloudY 1
        Add-Layer $g "$base/Cloud" 0 -.27 1
        Add-Layer $g "$base/Cloud" 20.405 -.27 1
        Add-Layer $g "$base/MidGround" 0 -2.425 1
        Add-Layer $g "$base/Hill" -4.375 -2.775 1
        if ($base -eq 'Faded') {
            Add-Layer $g 'Faded/Tree' 6.7775 .0975 1
        } else {
            Add-Layer $g 'Reborn/Tree' 0 0 1
        }
        Add-Layer $g "$base/BetweenFlowers" 0 -2.025 1
        Add-Layer $g "$base/Ground" 0 -2.81 1
        if ($state -ne 'Faded') { Add-Layer $g 'Reborn/Glow' 0 0 .24 }
        if ($state -eq 'Leaves') {
            Add-Layer $g 'Leaves/Shadow' 7.6075 .3275 .30
            Add-Layer $g 'Leaves/Crown' 2.725 1.485 1
        }
        $grassX = if ($base -eq 'Reborn') { .8875 } else { 0 }
        Add-Layer $g "$base/NearPlants" $grassX -2.85 1
        if ($base -eq 'Reborn') { Add-Layer $g 'Reborn/Fafa' -9.1425 -3.6 1 }
        $canvas.Save((Join-Path $outDir ($state + '.png')),
            [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $g.Dispose(); $canvas.Dispose() }
}
