Add-Type -AssemblyName System.Drawing
$bitmap = New-Object Drawing.Bitmap 1024,1024
$g = [Drawing.Graphics]::FromImage($bitmap)
$g.SmoothingMode = [Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [Drawing.Text.TextRenderingHint]::AntiAliasGridFit
$background = New-Object Drawing.Drawing2D.LinearGradientBrush ([Drawing.Point]::new(0,0)),([Drawing.Point]::new(1024,1024)),([Drawing.Color]::FromArgb(28,39,40)),([Drawing.Color]::FromArgb(9,14,17))
$g.FillRectangle($background,0,0,1024,1024)
$gold = [Drawing.Color]::FromArgb(232,181,88)
$light = [Drawing.Color]::FromArgb(244,230,201)
$border = New-Object Drawing.Pen $gold,8
$g.DrawRectangle($border,24,24,976,976)
$g.DrawLine($border,76,724,948,724)
$goldBrush = New-Object Drawing.SolidBrush $gold
$lightBrush = New-Object Drawing.SolidBrush $light
# An original geometric anvil and search-glass mark.
$points = [Drawing.Point[]]@([Drawing.Point]::new(160,275),[Drawing.Point]::new(628,275),[Drawing.Point]::new(628,355),[Drawing.Point]::new(563,383),[Drawing.Point]::new(536,464),[Drawing.Point]::new(578,500),[Drawing.Point]::new(578,545),[Drawing.Point]::new(292,545),[Drawing.Point]::new(292,500),[Drawing.Point]::new(340,464),[Drawing.Point]::new(328,383),[Drawing.Point]::new(210,349))
$g.FillPolygon($lightBrush,$points)
$dark = New-Object Drawing.SolidBrush ([Drawing.Color]::FromArgb(16,24,27))
$g.FillEllipse($dark,470,328,285,285)
$lens = New-Object Drawing.Pen $gold,33
$g.DrawEllipse($lens,476,334,273,273)
$handle = New-Object Drawing.Pen $gold,53
$handle.StartCap = [Drawing.Drawing2D.LineCap]::Round
$handle.EndCap = [Drawing.Drawing2D.LineCap]::Round
$g.DrawLine($handle,710,575,818,681)
$g.DrawLine($border,545,422,579,387)
$font1 = New-Object Drawing.Font 'Segoe UI',49,([Drawing.FontStyle]::Bold),([Drawing.GraphicsUnit]::Pixel)
$font2 = New-Object Drawing.Font 'Segoe UI',89,([Drawing.FontStyle]::Bold),([Drawing.GraphicsUnit]::Pixel)
$format = New-Object Drawing.StringFormat
$format.Alignment = [Drawing.StringAlignment]::Center
$g.DrawString('WORKSTATION',$font1,$lightBrush,([Drawing.RectangleF]::new(40,760,944,76)),$format)
$g.DrawString('SEARCH',$font2,$goldBrush,([Drawing.RectangleF]::new(40,824,944,126)),$format)
$small = New-Object Drawing.Bitmap 256,256
$sg = [Drawing.Graphics]::FromImage($small)
$sg.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$sg.DrawImage($bitmap,0,0,256,256)
$small.Save((Join-Path $PSScriptRoot 'icon.png'),[Drawing.Imaging.ImageFormat]::Png)
$sg.Dispose(); $small.Dispose(); $g.Dispose(); $bitmap.Dispose()
foreach ($resource in @($background,$border,$goldBrush,$lightBrush,$dark,$lens,$handle,$font1,$font2,$format)) { $resource.Dispose() }
