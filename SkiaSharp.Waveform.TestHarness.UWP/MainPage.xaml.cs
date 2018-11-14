//-----------------------------------------------------------------------
// <copyright file="MainPage.xaml.cs" company="Tom Alabaster">
//     Copyright (c) Tom Alabaster. All rights reserved.
// </copyright>
// <author>Tom Alabaster</author>
//-----------------------------------------------------------------------
namespace SkiaSharp.Waveform.TestHarness.UWP
{
    using System;
    using SkiaSharp.Views.UWP;
    using Windows.Media.Core;
    using Windows.Media.Playback;
    using Windows.System.Threading;
    using Windows.UI.Core;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// The <see cref="MediaPlayer"/> used for testing the <see cref="Waveform"/>.
        /// </summary>
        private MediaPlayer mediaPlayer;

        /// <summary>
        /// The <see cref="ThreadPoolTimer"/> used for redrawing the <see cref="Waveform"/>.
        /// </summary>
        private ThreadPoolTimer timer;

        /// <summary>
        /// The <see cref="Waveform"/> object that contains the drawing information of the waveform.
        /// </summary>
        private Waveform waveform;

        /// <summary>
        /// The sample rate that the waveform should draw for.
        /// </summary>
        private int sampleRate;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.InitializeWaveform();
        }

        /// <summary>
        /// Initialization of the waveform test harness components.
        /// </summary>
        private async void InitializeWaveform()
        {
            this.sampleRate = 44100;

            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(@"Assets\test.wav");

            this.waveform = new Waveform.Builder()
                .FromFile(file.Path, this.sampleRate)
                .WithScale(1f)
                .Build();
        }

        /// <summary>
        /// The event handler for the painting of the <see cref="SKXamlCanvas"/>.
        /// </summary>
        /// <param name="sender">The object sending the tap event.</param>
        /// <param name="e">The event arguments.</param>
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            this.waveform.DrawOnCanvas(e.Surface.Canvas);
        }

        /// <summary>
        /// The event handler for the Tap gesture on the canvasView.
        /// </summary>
        /// <param name="sender">The object sending the tap event.</param>
        /// <param name="e">The event arguments.</param>
        private void Canvas_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.mediaPlayer != null)
            {
                this.mediaPlayer.Dispose();
            }

            this.mediaPlayer = new MediaPlayer();
            this.mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/test.wav"));
            this.mediaPlayer.Play();

            if (this.timer == null)
            {
                this.timer = ThreadPoolTimer.CreatePeriodicTimer(
                    async (source) =>
                    {
                        await Dispatcher.RunAsync(
                            CoreDispatcherPriority.High,
                            () =>
                            {
                                this.waveform.Offset = (int)(this.mediaPlayer.PlaybackSession.Position.TotalSeconds * this.sampleRate);
                                ((SKXamlCanvas)this.FindName("canvasView")).Invalidate();
                            });
                    },
                    TimeSpan.FromMilliseconds(1000 / 60));
            }
        }
    }
}
