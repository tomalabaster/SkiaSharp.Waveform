//-----------------------------------------------------------------------
// <copyright file="TestHarnessViewController.cs" company="Tom Alabaster">
//     Copyright (c) Tom Alabaster. All rights reserved.
// </copyright>
// <author>Tom Alabaster</author>
//-----------------------------------------------------------------------
namespace Blank
{
    using System;
    using System.Timers;

    using Foundation;
    using SkiaSharp.Views.iOS;
    using SkiaSharp.Waveform;
    using UIKit;

    /// <summary>
    /// A test harness view controller for testing the functionality of the <see cref="Waveform"/> class.
    /// </summary>
    [Register("TestHarnessViewController")]
    public class TestHarnessViewController : UIViewController
    {
        /// <summary>
        /// The <see cref="SKCanvasView"/> that the test harness will be drawing too.
        /// </summary>
        private SKCanvasView canvasView;

        /// <summary>
        /// The <see cref="Waveform"/> object that contains the drawing information of the waveform.
        /// </summary>
        private Waveform waveform;

        /// <summary>
        /// The <see cref="Timer"/> object that handles the timing functions for drawing running waveforms.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestHarnessViewController"/> class.
        /// </summary>
        public TestHarnessViewController()
        {
        }

        /// <summary>
        /// <see cref="UIViewController.ViewDidLoad"/>
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.waveform = new Waveform()
            {
                Amplitudes = this.GetAmplitudeValues(),
                Scale = (float)UIScreen.MainScreen.Scale
            };

            this.canvasView = new SKCanvasView(this.View.Frame);
            this.canvasView.PaintSurface += this.CanvasView_PaintSurface;
            this.canvasView.AddGestureRecognizer(new UITapGestureRecognizer(this.CanvasView_Tapped));

            this.View.AddSubview(this.canvasView);
        }

        /// <summary>
        /// The event handler for the PaintSurface event of the <see cref="SKCanvasView"/> class.
        /// </summary>
        /// <param name="sender">The <see cref="SKCanvasView"/> that is calling the event.</param>
        /// <param name="e">The <see cref="SKPaintSurfaceEventArgs"/> arguments sent with the event.</param>
        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            this.waveform.DrawOnCanvas(e.Surface.Canvas);
        }

        /// <summary>
        /// The event handler for the Tap gesture on the canvasView.
        /// </summary>
        private void CanvasView_Tapped()
        {
            this.SetupTimer();
        }

        /// <summary>
        /// Used to create some random data for the waveform.
        /// </summary>
        /// <returns>Float array for values between 0 and 1.</returns>
        private float[] GetAmplitudeValues()
        {
            var random = new Random();

            var amplitudes = new float[1000];

            for (var i = 0; i < 1000; i++)
            {
                amplitudes[i] = (float)random.NextDouble();
            }

            return amplitudes;
        }

        /// <summary>
        /// Sets up the timer for drawing the waveform if it is playing.
        /// </summary>
        private void SetupTimer()
        {
            this.timer = new Timer();
            this.timer.Interval = 1000 / 24;
            this.timer.Enabled = true;
            this.timer.Elapsed += (object sender, ElapsedEventArgs e) =>
            {
                this.waveform.Offset += 1;
                BeginInvokeOnMainThread(this.canvasView.SetNeedsDisplay);
            };
        }
    }
}