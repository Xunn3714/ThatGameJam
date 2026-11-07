param(
    [string]$OutputDir = (Join-Path $PSScriptRoot '..\Assets\_Game\Resources\Level02')
)

$ErrorActionPreference = 'Stop'
$source = Join-Path $PSScriptRoot '..\Assets\_Game\Resources\Level02\卡住齿轮的石板.png'
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Add-Type -ReferencedAssemblies System.Drawing.Common, System.Drawing.Primitives, System.Private.Windows.GdiPlus, System.Private.Windows.Core -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

public static class SlabBreakFrames
{
    // The fracture follows the stone's existing notch. Both output canvases
    // retain every source pixel outside this narrow band.
    static readonly PointF[] Fracture = {
        new PointF(1237, 99), new PointF(1249, 120),
        new PointF(1231, 143), new PointF(1254, 162),
        new PointF(1240, 181), new PointF(1260, 201),
        new PointF(1243, 220), new PointF(1250, 240),
        new PointF(1226, 262), new PointF(1215, 283),
        new PointF(1200, 312)
    };

    static float Center(float y)
    {
        for (int i = 1; i < Fracture.Length; i++)
        {
            if (y <= Fracture[i].Y)
            {
                PointF a = Fracture[i - 1], b = Fracture[i];
                float t = Math.Max(0f, (y - a.Y) / (b.Y - a.Y));
                return a.X + (b.X - a.X) * t;
            }
        }
        return Fracture[Fracture.Length - 1].X;
    }

    public static void Build(string originalPath, string crackedPath, string brokenPath)
    {
        using (Bitmap original = new Bitmap(originalPath))
        using (Bitmap cracked = new Bitmap(original))
        using (Bitmap broken = new Bitmap(original))
        {
            for (int y = 99; y <= 312; y++)
            {
                float center = Center(y);
                int start = Math.Max(0, (int)center - 40);
                int end = Math.Min(original.Width - 1, (int)center + 40);
                float gap = 18f + (y > 143 && y < 260 ? 5f : 0f);
                for (int x = start; x <= end; x++)
                {
                    Color pixel = original.GetPixel(x, y);
                    if (pixel.A == 0) continue;
                    float distance = Math.Abs(x - center);

                    // Frame one is still one stone: a dark hairline and small
                    // chipped edge stop short of its lower lip.
                    if (y < 278)
                    {
                        if (distance <= 3.5f)
                            cracked.SetPixel(x, y, Color.FromArgb(pixel.A, 7, 32, 50));
                        else if (distance <= 6f && x < center)
                            cracked.SetPixel(x, y, Color.FromArgb(pixel.A, 46, 98, 109));
                    }

                    // Frame two is cut through, with a transparent jagged gap.
                    if (distance < gap)
                        broken.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
                    else if (distance < gap + 4f)
                    {
                        int r = Math.Min(255, pixel.R + 14);
                        int g = Math.Min(255, pixel.G + 13);
                        int b = Math.Min(255, pixel.B + 10);
                        broken.SetPixel(x, y, Color.FromArgb(pixel.A, r, g, b));
                    }
                }
            }

            // A few chips made from the source palette sit inside the gap.
            using (Graphics graphics = Graphics.FromImage(broken))
            using (Brush blue = new SolidBrush(Color.FromArgb(21, 62, 95)))
            using (Brush gray = new SolidBrush(Color.FromArgb(109, 108, 98)))
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.FillPolygon(blue, new[] {
                    new Point(1244, 150), new Point(1251, 147),
                    new Point(1255, 153), new Point(1248, 158) });
                graphics.FillPolygon(gray, new[] {
                    new Point(1249, 185), new Point(1257, 182),
                    new Point(1259, 189), new Point(1252, 192) });
                graphics.FillPolygon(blue, new[] {
                    new Point(1240, 233), new Point(1246, 229),
                    new Point(1250, 236), new Point(1243, 240) });
            }

            cracked.Save(crackedPath, ImageFormat.Png);
            broken.Save(brokenPath, ImageFormat.Png);
        }
    }
}
'@

[SlabBreakFrames]::Build(
    (Resolve-Path -LiteralPath $source).Path,
    (Join-Path $OutputDir '卡齿石板_裂开.png'),
    (Join-Path $OutputDir '卡齿石板_破开.png'))
