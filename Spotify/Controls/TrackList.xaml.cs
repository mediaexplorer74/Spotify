// Track List

using System;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls
{
    /// <summary>
    /// Class for listing the details of a track in a row
    /// </summary>
    public sealed partial class TrackList : UserControl
    {
        public Track track;

        /// <summary>
        /// The main constructor
        /// </summary>
        public TrackList()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Constructor including references to the track whose information
        /// will be displayed as well as the MainPage from where it was created
        /// </summary>
        /// <param name="playlist">The Track whose information will be displayed</param>
        /// <param name="mainPage">The MainPage containing the Playlist</param>
        public TrackList(Track track) : this()
        {
            this.track = track;
            PopulateData();
        }

        /// <summary>
        /// Populate UI with Track information
        /// </summary>
        private void PopulateData()
        {
            Image.Source = track.album.image;
            DisplayName.Text = track.name;
            Artist.Text = track.GetMainArtistName();
            Album.Text = track.album.GetMainArtistName();
            TimeSpan duration = TimeSpan.FromSeconds(Convert.ToDouble(track.duration) / 1000);
            if (duration.TotalHours < 1)
            {
                Duration.Text = (duration).ToString(@"mm\:ss");
            }
            else
            {
                Duration.Text = (Math.Floor(duration.TotalHours)).ToString() + ":" + (duration).ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// Sets light background for odd/even row distinguishability
        /// </summary>
        public void TurnOffOpaqueBackground()
        {
            Background.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// Free up memory
        /// </summary>
        public void Unload()
        {
            track.Dispose();

            Image.ClearValue(Image.SourceProperty);
        }
    }
}
