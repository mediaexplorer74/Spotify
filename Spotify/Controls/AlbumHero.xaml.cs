//

using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Spotify.Controls
{
    public sealed partial class AlbumHero : UserControl
    {
        public Album album;

        /// <summary>
        /// The main constructor
        /// </summary>
        public AlbumHero()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Constructor including references to the playlist whose information
        /// will be displayed as well as the MainPage from where it was created
        /// </summary>
        /// <param name="playlist">The Playlist whose information will be displayed</param>
        /// <param name="mainPage">The MainPage containing the Playlist</param>
        public AlbumHero(Album album) : this()
        {
            this.album = album;
            PopulateData();
        }

        /// <summary>
        /// Populate UI with Playlist information
        /// </summary>
        public void PopulateData()
        {
            Image.Source = album.image;
            DisplayName.Text = album.name;
            ArtistName.Text = album.GetMainArtistName();
            Tracks.Text = album.tracksCount.ToString();
        }

        /// <summary>
        /// Free up memory
        /// </summary>
        public void Unload()
        {
            album.Dispose();

            Image.ClearValue(Image.SourceProperty);
        }
    }
}
