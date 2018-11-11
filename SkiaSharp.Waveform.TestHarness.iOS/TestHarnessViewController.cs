//-----------------------------------------------------------------------
// <copyright file="TestHarnessViewController.cs" company="Tom Alabaster">
//     Copyright (c) Tom Alabaster. All rights reserved.
// </copyright>
// <author>Tom Alabaster</author>
//-----------------------------------------------------------------------
namespace Blank
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;
    using AVFoundation;
    using CoreAnimation;
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
        private AVAudioPlayer player;
        private NSError error;
        private NSUrl url;

        /// <summary>
        /// The <see cref="SKCanvasView"/> that the test harness will be drawing too.
        /// </summary>
        private SKCanvasView canvasView;

        /// <summary>
        /// The <see cref="Waveform"/> object that contains the drawing information of the waveform.
        /// </summary>
        private Waveform waveform;

        /// <summary>
        /// The sample rate that the waveform should draw for.
        /// </summary>
        private int sampleRate;

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

            this.sampleRate = 44100;

            this.waveform = new Waveform.Builder()
                .FromFile(Path.Combine(NSBundle.MainBundle.ResourcePath, "test.wav"), this.sampleRate)
                .WithScale((float)UIScreen.MainScreen.Scale)
                .Build();

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
            this.SetupPlayback();
            this.player.Play();
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
            var link = CADisplayLink.Create(() =>
            {
                this.waveform.Offset = (int)(this.player.CurrentTime * this.sampleRate);
                BeginInvokeOnMainThread(this.canvasView.SetNeedsDisplay);
            });

            link.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Default);
        }

        private bool SetupPlayback()
        {
            var audioSession = AVAudioSession.SharedInstance();

            this.error = audioSession.SetCategory(AVAudioSessionCategory.Playback);

            if (this.error != null)
            {
                return false;
            }

            this.error = audioSession.SetActive(true);

            if (this.error != null)
            {
                return false;
            }

            this.url = NSUrl.FromFilename(Path.Combine(NSBundle.MainBundle.ResourcePath, "test.wav"));

            try
            {
                this.player = AVAudioPlayer.FromUrl(this.url, out this.error);
            }
            catch
            {
                return false;
            }

            this.player.PrepareToPlay();
            this.player.MeteringEnabled = true;

            return this.error == null;
        }
    }
}