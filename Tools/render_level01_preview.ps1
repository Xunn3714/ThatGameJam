# Approximate saved-scene composition at the end of the opening camera zoom.
# Uses the exact imported PNGs and serialized transforms; Unity's sprite mesh and shader may differ.
param(
    [double]$CameraX = 8.9,
    [double]$CameraY = 3.4,
    [string]$OutputName = 'level01_layout_preview.png'
)
Add-Type -AssemblyName System.Drawing
$projectRoot = Split-Path $PSScriptRoot -Parent
$scene = Get-Content (Join-Path $projectRoot 'Assets/_Game/Scenes/Level01_Alpha.unity') -Raw
$blocks = [regex]::Split($scene, '(?m)(?=^--- !u!\d+ &\d+)')
$gos = @{}
$trs = @{}
$renderers = @()
$speeds = @{}

foreach ($block in $blocks) {
    if ($block -match '^--- !u!(\d+) &(\d+)') {
        $class = [int]$Matches[1]; $id = $Matches[2]
        if ($class -eq 1) {
            $n = [regex]::Match($block, '(?m)^  m_Name: (.*)$')
            $a = [regex]::Match($block, '(?m)^  m_IsActive: (\d+)$')
            $gos[$id] = @{ Name=$n.Groups[1].Value.Trim(); Active=($a.Groups[1].Value -eq '1') }
        } elseif ($class -eq 4) {
            $g = [regex]::Match($block, 'm_GameObject: \{fileID: (\d+)\}')
            $p = [regex]::Match($block, 'm_LocalPosition: \{x: ([^,]+), y: ([^,]+), z:')
            $s = [regex]::Match($block, 'm_LocalScale: \{x: ([^,]+), y: ([^,]+), z:')
            $f = [regex]::Match($block, 'm_Father: \{fileID: (\d+)\}')
            $trs[$id] = @{ Go=$g.Groups[1].Value; X=[double]::Parse($p.Groups[1].Value,[cultureinfo]::InvariantCulture); Y=[double]::Parse($p.Groups[2].Value,[cultureinfo]::InvariantCulture); SX=[double]::Parse($s.Groups[1].Value,[cultureinfo]::InvariantCulture); SY=[double]::Parse($s.Groups[2].Value,[cultureinfo]::InvariantCulture); Father=$f.Groups[1].Value }
        } elseif ($class -eq 212) {
            $g = [regex]::Match($block, 'm_GameObject: \{fileID: (\d+)\}')
            $sp = [regex]::Match($block, 'm_Sprite: \{fileID: \d+, guid: ([0-9a-f]{32})')
            $o = [regex]::Match($block, 'm_SortingOrder: (-?\d+)')
            $e = [regex]::Match($block, '(?m)^  m_Enabled: (\d+)')
            $d = [regex]::Match($block, 'm_DrawMode: (\d+)')
            $sz = [regex]::Match($block, 'm_Size: \{x: ([^,]+), y: ([^}]+)\}')
            if ($sp.Success) { $renderers += @{ Go=$g.Groups[1].Value; Guid=$sp.Groups[1].Value; Order=[int]$o.Groups[1].Value; Enabled=($e.Groups[1].Value -eq '1'); Draw=[int]$d.Groups[1].Value; W=[double]::Parse($sz.Groups[1].Value,[cultureinfo]::InvariantCulture); H=[double]::Parse($sz.Groups[2].Value,[cultureinfo]::InvariantCulture) } }
        } elseif ($class -eq 114 -and $block.Contains('SeedGame.ParallaxLayer')) {
            $g = [regex]::Match($block, 'm_GameObject: \{fileID: (\d+)\}')
            $x = [regex]::Match($block, 'horizontalScreenSpeed: ([^\r\n]+)')
            $y = [regex]::Match($block, 'verticalScreenSpeed: ([^\r\n]+)')
            $speeds[$g.Groups[1].Value] = @([double]::Parse($x.Groups[1].Value,[cultureinfo]::InvariantCulture), [double]::Parse($y.Groups[1].Value,[cultureinfo]::InvariantCulture))
        }
    }
}

$transformForGo = @{}
foreach ($id in $trs.Keys) { $transformForGo[$trs[$id].Go] = $id }
$guidToImage = @{}
foreach ($meta in (Get-ChildItem (Join-Path $projectRoot 'Assets') -Recurse -File -Filter '*.png.meta')) {
    $m = [regex]::Match((Get-Content $meta.FullName -Raw), '(?m)^guid: ([0-9a-f]{32})$')
    if ($m.Success) { $guidToImage[$m.Groups[1].Value] = $meta.FullName.Substring(0,$meta.FullName.Length - 5) }
}

$canvas = [System.Drawing.Bitmap]::new(1780,1000)
$gfx = [System.Drawing.Graphics]::FromImage($canvas)
$gfx.Clear([System.Drawing.Color]::FromArgb(231,228,219))
$gfx.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$gfx.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver

foreach ($r in ($renderers | Sort-Object Order)) {
    if (!$r.Enabled -or !$guidToImage.ContainsKey($r.Guid)) { continue }
    $trid = $transformForGo[$r.Go]
    if (!$trid) { continue }
    $x=0.0; $y=0.0; $sx=1.0; $sy=1.0; $active=$true; $speed=$null
    $chain = @(); $cursor=$trid
    while ($cursor -and $cursor -ne '0') { $chain = @($cursor) + $chain; $cursor=$trs[$cursor].Father }
    foreach ($part in $chain) {
        $t=$trs[$part]; $x += $t.X * $sx; $y += $t.Y * $sy; $sx *= $t.SX; $sy *= $t.SY
        if (!$gos[$t.Go].Active) { $active=$false }
        if ($speeds.ContainsKey($t.Go)) { $speed=$speeds[$t.Go] }
    }
    if (!$active) { continue }
    if ($speed) { $x += ($CameraX-2) * (1-$speed[0]); $y += ($CameraY-2.1) * (1-$speed[1]) }
    $path=$guidToImage[$r.Guid]
    $im=[System.Drawing.Image]::FromFile($path)
    $meta=Get-Content ($path+'.meta') -Raw
    $ppuMatch=[regex]::Match($meta,'spritePixelsToUnits: ([0-9.]+)')
    $ppu=[double]::Parse($ppuMatch.Groups[1].Value,[cultureinfo]::InvariantCulture)
    if ($r.Draw -ne 0) { $w=$r.W*$sx; $h=$r.H*$sy } else { $w=$im.Width/$ppu*$sx; $h=$im.Height/$ppu*$sy }
    $dest=[System.Drawing.RectangleF]::new([float](($x-$w/2-($CameraX-8.9))*100),[float](($CameraY+5-$y-$h/2)*100),[float]($w*100),[float]($h*100))
    if ($dest.Right -gt 0 -and $dest.Left -lt 1780 -and $dest.Bottom -gt 0 -and $dest.Top -lt 1000) { $gfx.DrawImage($im,$dest) }
    $im.Dispose()
}
$gfx.Dispose()
$out=Join-Path $PSScriptRoot $OutputName
$canvas.Save($out,[System.Drawing.Imaging.ImageFormat]::Png)
$canvas.Dispose()
Write-Output $out
