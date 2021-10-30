//

using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls
{
    /// <summary>
    /// Class for listing the details of an album in a row
    /// </summary>
    public sealed partial class AlbumList : UserControl
    {
        public Album album;

        /// <summary>
        /// The main constructor
        /// </summary>
        public AlbumList()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Constructor including references to the album whose information
        /// will be displayed as well as the MainPage from where it was created
        /// </summary>
        /// <param name="playlist">The Album whose information will be displayed</param>
        /// <param name="mainPage">The MainPage containing the Playlist</param>
        public AlbumList(Album album) : this()
        {
            this.album = album;
            PopulateData();
        }

        /// <summary>
        /// Populate UI with Album information
        /// </summary>
        public void PopulateData()
        {
            Image.Source = album.image;
            DisplayName.Text = album.name;
            Artist.Text = album.GetMainArtistName();
            ReleaseDate.Text = album.releaseDate;
            Tracks.Text = album.tracksCount.ToString();
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
        public async void Unload()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                album.Dispose();

                Image.ClearValue(Image.SourceProperty);
            });
        }
    }
}
