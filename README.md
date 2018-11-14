# SkiaSharp.Waveform

**SkiaSharp.Waveform** is a library for drawing waveforms using SkiaSharp.

```c#
var waveform = new Waveform()
{
    Amplitudes = amplitudes, // float array with values between 0 and 1
    Scale = 3f, // device screen scale factor (optional, default = 1f)
    Spacing = 5f // spacing between plotted points (optional, default = 1f),
    Color = new SKColor(0x00, 0x00, 0xff)
};

skCanvasView.PaintSurface += (object sender, SKPaintSurfaceEventArgs e)
{
    waveform.DrawOnCanvas(e.Surface.Canvas);
}
```

There is now a ```Waveform.Builder``` class to use which has fluent methods.

```c#
var waveform = new Waveform.Builder()
                .WithAmplitudes(amplitudes)
                .WithScale((float)UIScreen.MainScreen.Scale)
                .WithSpacing(5f)
                .WithColor(new SKColor(0x00, 0x00, 0xff))
                .Build();
```

It is now possible to generate a waveform from a WAV file using the ```Builder```.

```c#
var path = Path.Combine(NSBundle.MainBundle.ResourcePath, "test.wav"); // absolute path to your WAV file

var sampleRate = 22500; // sample rate at which to read the file, doesn't have to be the rate at which it was recorded

var waveform = new Waveform.Builder()
                .WithScale((float)UIScreen.MainScreen.Scale)
                .FromFile(path, sampleRate)
                .Build();
```