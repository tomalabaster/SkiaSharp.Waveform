# SkiaSharp.Waveform

**SkiaSharp.Waveform** is a library for drawing waveforms using SkiaSharp.

```c#
var waveform = new Waveform()
{
    Amplitudes = amplitudes, // float array with values between 0 and 1
    Scale = 3f, // device screen scale factor (optional, default = 1f)
    Spacing = 5 // spacing between plotted points (optional, default = 1)
};

skCanvasView.PaintSurface += (object sender, SKPaintSurfaceEventArgs e)
{
    waveform.DrawOnCanvas(e.Surface.Canvas);
}
```
