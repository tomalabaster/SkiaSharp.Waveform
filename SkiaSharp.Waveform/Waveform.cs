//-----------------------------------------------------------------------
// <copyright file="Waveform.cs" company="Tom Alabaster">
//     Copyright (c) Tom Alabaster. All rights reserved.
// </copyright>
// <author>Tom Alabaster</author>
//-----------------------------------------------------------------------
namespace SkiaSharp.Waveform
{
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
        /// Gets or sets the scale of the waveform. This should be based on screen density.
        /// </summary>
        public float Scale { get; set; } = 1f;

        /// <summary>
        /// Gets or sets an array of normalized amplitude values;
        /// </summary>
        public float[] Amplitudes { get; set; }

        /// <summary>
        /// Gets or sets the spacing between each plotted amplitude.
        /// </summary>
        public int Spacing { get; set; } = 1;

        /// <summary>
        /// Gets or sets the index offset of the amplitude values.
        /// </summary>
        public int Offset { get; set; } = 0;

        /// <summary>
        /// Gets or sets the width of the stroke.
        /// </summary>
        public int StrokeWidth { get; set; }

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
                Color = new SKColor(0x00, 0x00, 0xff),
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
    }
}