// PlaylistList

using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls
{
    /// <summary>
    /// A class for listing the details of a playlist in a row
    /// </summary>
    public sealed partial class PlaylistList : UserControl
    {
        public Playlist playlist;

        /// <summary>
        /// The main constructor
        /// </summary>
        public PlaylistList()
        {
            this.InitializeComponent();
        }
        
        /// <summary>
        /// Constructor including references to the playlist whose information
        /// will be displayed as well as the MainPage from where it was created
        /// </summary>
        /// <param name="playlist">The Playlist whose information will be displayed</param>
        /// <param name="mainPage">The MainPage containing the Playlist</param>
        public PlaylistList(Playlist playlist) : this()
        {
            this.playlist = playlist;
            PopulateData();
        }

        /// <summary>
        /// Populate UI with Playlist information
        /// </summary>
        public void PopulateData()
        {
            Image.Source = playlist.image;
            DisplayName.Text = playlist.name;
            DisplayDesc.Text = playlist.description;
            DisplayOwner.Text = playlist.owner;
            Tracks.Text = playlist.tracksCount.ToString();
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
            playlist.Dispose();

            Image.ClearValue(Image.SourceProperty);
        }
    }
}
