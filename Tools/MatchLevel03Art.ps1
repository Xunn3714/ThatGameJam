param(
    [Parameter(Mandatory=$true)][string]$Reference,
    [Parameter(Mandatory=$true)][string]$Layer,
    [int]$XMin = -100,
    [int]$XMax = 100,
    [int]$YMin = 0,
    [int]$YMax = 214
)

Add-Type -AssemblyName System.Drawing
if (-not ('Level03Matcher' -as [type])) {
Add-Type -ReferencedAssemblies @(
    [System.Drawing.Image].Assembly.Location,
    (Join-Path (Split-Path ([System.Drawing.Image].Assembly.Location)) 'System.Private.Windows.GdiPlus.dll'),
    (Join-Path (Split-Path ([System.Drawing.Image].Assembly.Location)) 'System.Private.Windows.Core.dll'),
    [System.Drawing.Color].Assembly.Location,
    [System.Object].Assembly.Location,
    [System.Collections.Generic.List[int]].Assembly.Location,
    (Join-Path (Split-Path ([System.Drawing.Image].Assembly.Location)) 'System.Collections.dll'),
    (Join-Path (Split-Path ([System.Drawing.Image].Assembly.Location)) 'System.Runtime.dll')
) -TypeDefinition @'
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public static class Level03Matcher {
    static byte[] Pixels(Image image, out int width, out int height) {
        width = Math.Max(1, image.Width / 8);
        height = Math.Max(1, image.Height / 8);
        using (var thumb = new Bitmap(width, height, PixelFormat.Format32bppArgb)) {
            using (var g = Graphics.FromImage(thumb)) {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, width, height);
            }
            var rect = new Rectangle(0, 0, width, height);
            var data = thumb.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try {
                var bytes = new byte[width * height * 4];
                Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                return bytes;
            } finally { thumb.UnlockBits(data); }
        }
    }
    public static string Find(string reference, string layer, int xmin, int xmax, int ymin, int ymax) {
        using (var r = Image.FromFile(reference))
        using (var l = Image.FromFile(layer)) {
            int rw, rh, lw, lh;
            var rb = Pixels(r, out rw, out rh);
            var lb = Pixels(l, out lw, out lh);
            var points = new int[lw*lh];
            int pointCount=0;
            for (int y=2; y<lh-2; y+=2)
                for (int x=2; x<lw-2; x+=2) {
                    int j=(y*lw+x)*4;
                    if (lb[j+3] >= 235) points[pointCount++]=y*lw+x;
                }
            int best=-1, bx=0, by=0;
            for (int oy=ymin; oy<=ymax; oy++)
                for (int ox=xmin; ox<=xmax; ox++) {
                    int hits=0;
                    for (int pi=0; pi<pointCount; pi++) {
                        int p=points[pi];
                        int x=p%lw+ox, y=p/lw+oy;
                        if ((uint)x >= rw || (uint)y >= rh) continue;
                        int a=p*4, b=(y*rw+x)*4;
                        int d0=lb[a]-rb[b], d1=lb[a+1]-rb[b+1], d2=lb[a+2]-rb[b+2];
                        if (d0*d0+d1*d1+d2*d2 < 1800) hits++;
                    }
                    if (hits>best) { best=hits; bx=ox; by=oy; }
                }
            return "offset@1/8="+bx+","+by+"; original="+(bx*8)+","+(by*8)+"; hits="+best+"/"+pointCount;
        }
    }
}
'@
}
[Level03Matcher]::Find($Reference, $Layer, $XMin, $XMax, $YMin, $YMax)
