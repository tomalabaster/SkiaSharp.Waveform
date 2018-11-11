//-----------------------------------------------------------------------
// <copyright file="Waveform.cs" company="Tom Alabaster">
//     Copyright (c) Tom Alabaster. All rights reserved.
// </copyright>
// <author>Tom Alabaster</author>
//-----------------------------------------------------------------------
namespace SkiaSharp.Waveform
{
    using System.IO;
    using System.Linq;
    using SkiaSharp;

    /// <summary>
    /// The Waveform class handles the drawing of the waveform onto a given SKCanvas.
    /// </summary>
    public class Waveform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Waveform"/> class.
        /// </summary>
        public Waveform()
        {
        }

        /// <summary>
        /// Gets or sets an array of normalized amplitude values.
        /// </summary>
        public float[] Amplitudes { get; set; } = new float[0];

        /// <summary>
        /// Gets or sets the scale of the waveform. This should be based on screen density.
        /// </summary>
        public float Scale { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the spacing between each plotted amplitude.
        /// </summary>
        public float Spacing { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the index offset of the amplitude values.
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the width of the stroke.
        /// </summary>
        public float StrokeWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the color of the waveform.
        /// </summary>
        public SKColor Color { get; set; } = SKColors.Blue;

        /// <summary>
        /// Gets the scaled spacing.
        /// </summary>
        public float ScaledSpacing => this.Scale * this.Spacing;

        /// <summary>
        /// Draw the waveform on the specified <paramref name="canvas"/>.
        /// </summary>
        /// <param name="canvas">The <see cref="SKCanvas"/> to draw the waveform on.</param>
        public void DrawOnCanvas(SKCanvas canvas)
        {
            canvas.Clear(SKColors.White);

            var dimensions = canvas.DeviceClipBounds;
            var midpoint = dimensions.Height / 2;
            var numberOfPointsToPlot = dimensions.Width / this.ScaledSpacing;

            var paint = this.BuildPaint();

            var path = new SKPath();
            path.MoveTo(new SKPoint(0, midpoint));

            for (var i = 0; i < numberOfPointsToPlot; i++)
            {
                var amplitudeIndex = i + this.Offset;
                var amplitudeValue = amplitudeIndex >= this.Amplitudes.Length ? 0 : this.Amplitudes[amplitudeIndex];

                var multiplier = this.GetUpOrDownMultiplier(amplitudeIndex);

                var controlPoint = new SKPoint()
                {
                    X = ((i - 1) * this.ScaledSpacing) + (this.ScaledSpacing / 2),
                    Y = midpoint + ((amplitudeValue * 100) * multiplier)
                };

                var amplitudePoint = new SKPoint()
                {
                    X = i * this.ScaledSpacing,
                    Y = midpoint
                };

                path.QuadTo(controlPoint, amplitudePoint);
            }

            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Constructs the paint object to use to draw the waveform.
        /// </summary>
        /// <returns><see cref="SKPaint"/></returns>
        private SKPaint BuildPaint()
        {
            return new SKPaint()
            {
                IsAntialias = true,
                Color = this.Color,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = this.StrokeWidth * this.Scale
            };
        }

        /// <summary>
        /// Determines whether or not an amplitude value is to be drawn above or below the middle line.
        /// </summary>
        /// <param name="amplitudeIndex">The amplitude index which is made up of the offset and the current index in the points to plot loop.</param>
        /// <returns><see cref="int"/></returns>
        private int GetUpOrDownMultiplier(int amplitudeIndex)
        {
            return amplitudeIndex % 2 == 0 ? 1 : -1;
        }


        /// <summary>
        /// Opens a WAV file by extracting the peaks for left and right channels into double arrays.
        /// </summary>
        /// <param name="filePath">The absolute path to the WAV file to open into left and right double arrays.</param>
        /// <param name="left">The array of peaks for the left channel from the WAV file.</param>
        /// <param name="right">The array of peaks for the right channel from the WAV file.</param>
        private void OpenWav(string filePath, out double[] left, out double[] right)
        {
            byte[] wav = File.ReadAllBytes(filePath);

            left = new double[wav.Length];
            right = new double[wav.Length];

            // Determine if mono or stereo
            int channels = wav[22];     // Forget byte 23 as 99.999% of WAVs are 1 or 2 channels

            // Get past all the other sub chunks to get to the data subchunk:
            int pos = 12;   // First Subchunk ID from 12 to 16

            // Keep iterating until we find the data chunk (i.e. 64 61 74 61 ...... (i.e. 100 97 116 97 in decimal))
            while (!(wav[pos] == 100 && wav[pos + 1] == 97 && wav[pos + 2] == 116 && wav[pos + 3] == 97))
            {
                pos += 4;
                int chunkSize = wav[pos] + wav[pos + 1] * 256 + wav[pos + 2] * 65536 + wav[pos + 3] * 16777216;
                pos += 4 + chunkSize;
            }
            pos += 8;

            // Pos is now positioned to start of actual sound data.
            int samples = (wav.Length - pos) / 2;     // 2 bytes per sample (16 bit sound mono)
            if (channels == 2) samples /= 2;        // 4 bytes per sample (16 bit stereo)

            // Allocate memory (right will be null if only mono sound)
            left = new double[samples];
            if (channels == 2) right = new double[samples];
            else right = null;

            // Write to double array/s:
            int i = 0;
            while (pos < wav.Length)
            {
                left[i] = this.BytesToDouble(wav[pos], wav[pos + 1]);
                pos += 2;
                if (channels == 2)
                {
                    right[i] = this.BytesToDouble(wav[pos], wav[pos + 1]);
                    pos += 2;
                }
                i++;
            }

            left = left.Select(x => x * 10).ToArray();
            right = left.Select(x => x * 10).ToArray();
        }

        /// <summary>
        /// Converts bytes to doubles.
        /// </summary>
        /// <returns>The result of converting the bytes to the double.</returns>
        /// <param name="firstByte">First byte.</param>
        /// <param name="secondByte">Second byte.</param>
        private double BytesToDouble(byte firstByte, byte secondByte)
        {
            short s = (short)((secondByte << 8) | firstByte);
            return s / 32768.0;
        }

        /// <summary>
        /// The builder class which enables the creation of a Waveform in a fluent manner.
        /// </summary>
        public class Builder
        {
            /// <summary>
            /// The Waveform being constructed by the builder.
            /// </summary>
            private Waveform waveform;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:SkiaSharp.Waveform.Waveform.Builder"/> class.
            /// </summary>
            public Builder()
            {
                this.waveform = new Waveform();
            }

            /// <summary>
            /// Sets the amplitudes for the Waveform to draw.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="amplitudes">Amplitudes.</param>
            public Builder WithAmplitudes(float[] amplitudes)
            {
                this.waveform.Amplitudes = amplitudes;

                return this;
            }

            /// <summary>
            /// Sets the scale of the Waveform. This should be based on the screen density of the target display.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="scale">Scale.</param>
            public Builder WithScale(float scale)
            {
                this.waveform.Scale = scale;

                return this;
            }

            /// <summary>
            /// Sets the spacing between peaks.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="spacing">Spacing.</param>
            public Builder WithSpacing(float spacing)
            {
                this.waveform.Spacing = spacing;

                return this;
            }

            /// <summary>
            /// Sets the width of the stroke to draw with.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="strokeWidth">Stroke width.</param>
            public Builder WithStrokeWidth(float strokeWidth)
            {
                this.waveform.StrokeWidth = strokeWidth;

                return this;
            }

            /// <summary>
            /// Sets the color.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="color">Color.</param>
            public Builder WithColor(SKColor color)
            {
                this.waveform.Color = color;

                return this;
            }

            /// <summary>
            /// Takes in an absolute path to a WAV file with the sample rate at which to read it at.
            /// </summary>
            /// <returns>The Builder.</returns>
            /// <param name="filePath">File path.</param>
            /// <param name="sampleRate">Sample rate.</param>
            public Builder FromFile(string filePath, int sampleRate)
            {
                var left = new double[0];
                var right = new double[0];

                this.waveform.OpenWav(filePath, out left, out right);
                this.waveform.Amplitudes = left.Select(x => (float)x).Where((x, i) => i % (44100 / sampleRate) == 0).ToArray();

                return this;
            }

            /// <summary>
            /// Builds the Waveform from the properties specified in the fluent methods.
            /// </summary>
            /// <returns>The Waveform.</returns>
            public Waveform Build()
            {
                return this.waveform;
            }
        }
    }
}