// Playlist Hero

using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls
{
    public sealed partial class PlaylistHero : UserControl
    {
        public Playlist playlist;

        /// <summary>
        /// The main constructor
        /// </summary>
        public PlaylistHero()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Constructor including references to the playlist whose information
        /// will be displayed as well as the MainPage from where it was created
        /// </summary>
        /// <param name="playlist">The Playlist whose information will be displayed</param>
        /// <param name="mainPage">The MainPage containing the Playlist</param>
        public PlaylistHero(Playlist playlist) : this()
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
            Description.Text = playlist.description;
            Tracks.Text = playlist.tracksCount.ToString();
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
