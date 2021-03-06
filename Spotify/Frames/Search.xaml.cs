// Search

using Spotify.Classes;
using Spotify.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Spotify.Frames.Settings;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Spotify.Frames
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Search : Page
    {
        public static string searchSave;
        public static int searchTypeSave;
        public const String SEARCH_URL = "https://api.spotify.com/v1/search";

        public Search()
        {
            this.InitializeComponent();
            MainPage.searchPage = this;
            Feedback.Text = "";
            ResultsHeaderContainer.Visibility = Visibility.Collapsed;

            if (searchSave != null)
            {
                SearchBox.Text = searchSave;
                SearchType.SelectionChanged -= SearchButton_Click;
                SearchType.SelectedIndex = searchTypeSave;
                SearchType.SelectionChanged += SearchButton_Click;
                SearchButton_Click(SearchButton, null);
            }
        }

        /// <summary>
        /// Search for the selected item in Spotify
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string feedbackMessage = "";
            if (Feedback == null)
            {
                return;
            }
            Feedback.Text = "";
            feedbackMessage = "";
            PlaylistHeader.Visibility = Visibility.Collapsed;
            TracklistHeader.Visibility = Visibility.Collapsed;
            AlbumlistHeader.Visibility = Visibility.Collapsed;
            ResultsHeaderContainer.Visibility = Visibility.Collapsed;
            if (SearchBox.Text == "")
            {
                feedbackMessage = "Please enter text to search for (I can't read your mind...yet)";
            }
            else
            {
                searchSave = SearchBox.Text;
                searchTypeSave = SearchType.SelectedIndex;
                MainPanel.SetValue(MarginProperty, new Thickness(0,20,0,0));
                RelativePanel.SetAlignTopWithPanel(SearchBox, true);
                ComboBoxItem selected = SearchType.SelectedValue as ComboBoxItem;
                String selectedString = selected.Content.ToString().ToLower();
                UriBuilder searchUriBuilder = new UriBuilder(SEARCH_URL);
                List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("type", selectedString),
                    new KeyValuePair<string, string>("limit", "10"),
                    new KeyValuePair<string, string>("q", SearchBox.Text.Replace(" ", "+"))
                };
                string queryParamsString = RequestHandler.ConvertToQueryString(queryParams);
                searchUriBuilder.Query = queryParamsString;
                string searchResultString = await RequestHandler.SendCliGetRequest(searchUriBuilder.Uri.ToString());
                JsonObject searchResultJson = new JsonObject();
                try
                {
                    searchResultJson = JsonObject.Parse(searchResultString);
                }
                catch (COMException)
                {
                    return;
                }

                ClearResults();
                Results.Visibility = Visibility.Visible;

                // playlists
                if (selectedString == "playlist")
                {
                    if (searchResultJson.TryGetValue("playlists", out IJsonValue playlistsJson) && playlistsJson.ValueType == JsonValueType.Object)
                    {
                        JsonObject playlists = playlistsJson.GetObject();
                        if (playlists.TryGetValue("items", out IJsonValue itemsJson) && itemsJson.ValueType == JsonValueType.Array)
                        {
                            JsonArray playlistsArray = itemsJson.GetArray();
                            if (playlistsArray.Count == 0)
                            {
                                feedbackMessage = "No playlists found.";
                            }
                            else
                            {
                                ResultsHeaderContainer.Visibility = Visibility.Visible;
                                PlaylistHeader.Visibility = Visibility.Visible;
                                long loadingKey = DateTime.Now.Ticks;
                                MainPage.AddLoadingLock(loadingKey);
                                App.mainPage.SetLoadingProgress(PlaybackSource.Spotify, 0, playlistsArray.Count, loadingKey);
                                foreach (JsonValue playlistJson in playlistsArray)
                                {
                                    if (playlistJson.GetObject().TryGetValue("href", out IJsonValue fullHref) && fullHref.ValueType == JsonValueType.String)
                                    {
                                        string fullPlaylistString = await RequestHandler.SendCliGetRequest(fullHref.GetString());
                                        Playlist playlist = new Playlist();
                                        await playlist.SetInfo(fullPlaylistString);
                                        PlaylistList playlistList = new PlaylistList(playlist);
                                        try
                                        {
                                            if (!App.isInBackgroundMode)
                                            {
                                                Results.Items.Add(playlistList);
                                                if (Results.Items.IndexOf(playlistList) % 2 == 1)
                                                {
                                                    playlistList.TurnOffOpaqueBackground();
                                                }
                                            }
                                        }
                                        catch (COMException) { }
                                        App.mainPage.SetLoadingProgress(PlaybackSource.Spotify, Results.Items.Count, playlistsArray.Count, loadingKey);
                                    }
                                }
                                MainPage.RemoveLoadingLock(loadingKey);
                            }
                        }
                    }
                }

                // track
                else if (selectedString == "track")
                {
                    if (searchResultJson.TryGetValue("tracks", out IJsonValue tracksJson) && tracksJson.ValueType == JsonValueType.Object)
                    {
                        JsonObject tracks = tracksJson.GetObject();
                        if (tracks.TryGetValue("items", out IJsonValue itemsJson) && itemsJson.ValueType == JsonValueType.Array)
                        {
                            JsonArray tracksArray = itemsJson.GetArray();
                            if (tracksArray.Count == 0)
                            {
                                feedbackMessage = "No tracks found.";
                            }
                            else
                            {
                                ResultsHeaderContainer.Visibility = Visibility.Visible;
                                TracklistHeader.Visibility = Visibility.Visible;
                                long loadingKey = DateTime.Now.Ticks;
                                MainPage.AddLoadingLock(loadingKey);
                                App.mainPage.SetLoadingProgress(PlaybackSource.Spotify, 0, tracksArray.Count, loadingKey);
                                foreach (JsonValue trackJson in tracksArray)
                                {
                                    Track track = new Track();
                                    await track.SetInfoDirect(trackJson.Stringify());
                                    TrackList trackList = new TrackList(track);
                                    try
                                    {
                                        if (!App.isInBackgroundMode)
                                        {
                                            Results.Items.Add(trackList);
                                            if (Results.Items.IndexOf(trackList) % 2 == 1)
                                            {
                                                trackList.TurnOffOpaqueBackground();
                                            }
                                        }
                                    }
                                    catch (COMException) { }
                                    App.mainPage.SetLoadingProgress(PlaybackSource.Spotify, Results.Items.Count, tracksArray.Count, loadingKey);
                                }
                                MainPage.RemoveLoadingLock(loadingKey);
                            }
                        }
                    }
                }

                // album
                else if (selectedString == "album")
                {
                    if (searchResultJson.TryGetValue("albums", out IJsonValue albumsJson) && albumsJson.ValueType == JsonValueType.Object)
                    {
                        JsonObject albums = albumsJson.GetObject();
                        if (albums.TryGetValue("items", out IJsonValue itemsJson) && itemsJson.ValueType == JsonValueType.Array)
                        {
                            JsonArray albumsArray = itemsJson.GetArray();
                            if (albumsArray.Count == 0)
                            {
                                feedbackMessage = "No albums found.";
                            }
                            else
                            {
                                ResultsHeaderContainer.Visibility = Visibility.Visible;
                                AlbumlistHeader.Visibility = Visibility.Visible;
                                long loadingKey = DateTime.Now.Ticks;
                                MainPage.AddLoadingLock(loadingKey);
                                App.mainPage.SetLoadingProgress(PlaybackSource.Spotify, 0, albumsArray.Count, loadingKey);
                                foreach (JsonValue albumJson in albumsArray)
                                {
                                    Album album = new Album();
                                    await album.SetInfo(albumJson.Stringify());
                                    AlbumList albumList = new AlbumList(album);
                                    try
                                    {
                                        if (!App.isInBackgroundMode)
                                        {
                                            Results.Items.Add(albumList);
                                            if (Results.Items.IndexOf(albumList) % 2 == 1)
                                            {
                                                albumList.TurnOffOpaqueBackground();
                                            }
                                        }
                                    }
                                    catch (COMException) { }
                                    App.mainPage.SetLoadingProgress(PlaybackSource.Spotify, Results.Items.Count, albumsArray.Count, loadingKey);
                                }
                                MainPage.RemoveLoadingLock(loadingKey);
                            }
                        }
                    }
                }
            }
            Feedback.Text = feedbackMessage;
            if (feedbackMessage == "")
            {
                Feedback.Visibility = Visibility.Collapsed;
            }
            else
            {
                Feedback.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// User clicks result item to be played
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Results_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TrackList)
            {
                (e.ClickedItem as TrackList).track.PlayTrack();
            }
            else if (e.ClickedItem is PlaylistList)
            {
                (e.ClickedItem as PlaylistList).playlist.PlayTracks();
            }
            else if (e.ClickedItem is AlbumList)
            {
                (e.ClickedItem as AlbumList).album.PlayTracks();
            }
        }

        /// <summary>
        /// user hits enter to search
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SearchButton_Click(null, null);
            }
        }

        /// <summary>
        /// Clears the search results objects to purge them from memory
        /// </summary>
        private void ClearResults()
        {
            while (Results.Items.Count > 0)
            {
                object listItem = Results.Items.ElementAt(0);
                if (listItem is PlaylistList)
                {
                    PlaylistList playlistList = listItem as PlaylistList;
                    playlistList.Unload();
                    Results.Items.Remove(playlistList);
                }
                else if (listItem is TrackList)
                {
                    TrackList trackList = listItem as TrackList;
                    trackList.Unload();
                    Results.Items.Remove(trackList);
                }
                else if (listItem is AlbumList)
                {
                    AlbumList albumList = listItem as AlbumList;
                    albumList.Unload();
                    Results.Items.Remove(albumList);
                }
            }
        }

        /// <summary>
        /// Used when freeing memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (App.isInBackgroundMode)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Results.ItemClick -= Results_ItemClick;
                    ClearResults();

                    SearchType.SelectionChanged -= SearchButton_Click;
                    SearchButton.Click -= SearchButton_Click;
                });
            }
        }
    }
}
