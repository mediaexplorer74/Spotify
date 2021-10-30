// Playback Mode

using Spotify.Frames;
using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Spotify.Frames.Settings;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls.Announcements
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaybackMode : UserControl
    {
        /// <summary>
        /// Main constructor
        /// </summary>
        public PlaybackMode()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Construct the object with a pre-defined source
        /// </summary>
        /// <param name="source">The current source of the app</param>
        public PlaybackMode(PlaybackSource source) : this()
        {
            YouTube.Click -= PlaybackMode_Click;
            Spotify.Click -= PlaybackMode_Click;
            if (source == PlaybackSource.Spotify)
            {
                Spotify.IsChecked = true;
            }
            else if (source == PlaybackSource.YouTube)
            {
                YouTube.IsChecked = true;
            }
            YouTube.Click += PlaybackMode_Click;
            Spotify.Click += PlaybackMode_Click;
        }

        /// <summary>
        /// User clicks to change the playback source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlaybackMode_Click(object sender, RoutedEventArgs e)
        {
            PlaybackSource source = PlaybackSource.Spotify;
            if (Spotify.IsChecked == true)
            {
                source = PlaybackSource.Spotify;
            }
            else if (YouTube.IsChecked == true)
            {
                source = PlaybackSource.YouTube;
            }
            if (MainPage.settingsPage != null)
            {
                MainPage.settingsPage.SetPlaybackSourceUI(source);
            }
            else
            {
                SetPlaybackSource(source);
            }
        }

        /// <summary>
        /// Free up memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (MainPage.closedAnnouncements || (App.isInBackgroundMode && MainPage.closedAnnouncements))
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Spotify.Click -= PlaybackMode_Click;
                    YouTube.Click -= PlaybackMode_Click;
                });
            }
        }
    }
}
