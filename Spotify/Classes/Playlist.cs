﻿// Playlist

using Spotify.Classes;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml.Media.Imaging;

namespace Spotify
{
    /// <summary>
    /// A playlist object containing tracks
    /// </summary>
    public class Playlist : IDisposable
    {
        private bool disposed = false;
        public string id = "";
        public string href = "";
        public string name = "";
        public string owner = "";
        public string description = "";
        public string tracksHref = "";
        private const int maxTracksPerRequest = 100;
        public int tracksCount = 0;
        public BitmapImage image = new BitmapImage();

        /// <summary>
        /// The main constructor to create an empty instance
        /// </summary>
        public Playlist()
        {

        }

        /// <summary>
        /// Populate the playlists information from the JSON object
        /// </summary>
        /// <param name="jsonString">The string representation of the playlist JSON object</param>
        /// <returns></returns>
        public async Task SetInfo(string jsonString)
        {
            JsonObject playlistJson = new JsonObject();
            try
            {
                playlistJson = JsonObject.Parse(jsonString);
            }
            catch (COMException)
            {
                return;
            }
            if (playlistJson.TryGetValue("id", out IJsonValue idJson) && idJson.ValueType == JsonValueType.String)
            {
                id = idJson.GetString();
            }
            if (playlistJson.TryGetValue("href", out IJsonValue hrefJson) && hrefJson.ValueType == JsonValueType.String)
            {
                href = hrefJson.GetString();
            }
            if (playlistJson.TryGetValue("name", out IJsonValue nameJson) && nameJson.ValueType == JsonValueType.String)
            {
                name = nameJson.GetString();
            }
            if (playlistJson.TryGetValue("owner", out IJsonValue ownerJson) && ownerJson.ValueType == JsonValueType.Object)
            {
                if (ownerJson.GetObject().TryGetValue("id", out IJsonValue ownerIdJson) && ownerIdJson.ValueType == JsonValueType.String)
                {
                    owner = ownerIdJson.GetString();
                }
            }
            if (playlistJson.TryGetValue("description", out IJsonValue descriptionJson) && descriptionJson.ValueType == JsonValueType.String)
            {
                description = Regex.Replace(descriptionJson.GetString(), "<.+?>", string.Empty);
            }

            if (playlistJson.TryGetValue("tracks", out IJsonValue tracksJson) && tracksJson.ValueType == JsonValueType.Object)
            {
                JsonObject trackJson = tracksJson.GetObject();
                if (trackJson.TryGetValue("href", out IJsonValue trackHrefJson) && trackHrefJson.ValueType == JsonValueType.String)
                {
                    tracksHref = trackHrefJson.GetString();
                }
                if (trackJson.TryGetValue("total", out IJsonValue trackNumberJson) && trackNumberJson.ValueType == JsonValueType.Number)
                {
                    tracksCount = Convert.ToInt32(trackNumberJson.GetNumber());
                }
            }
            if (playlistJson.TryGetValue("images", out IJsonValue imagesJson) && imagesJson.ValueType == JsonValueType.Array)
            {
                JsonArray imagesArray = imagesJson.GetArray();
                if (imagesArray.Count > 0)
                {
                    JsonValue imageObject = imagesArray.ElementAt(0) as JsonValue;
                    JsonValue urlJson = imageObject.GetObject().GetNamedValue("url");
                    string url = urlJson.GetString();
                    image = await RequestHandler.DownloadImage(url);
                }
            }
        }

        /// <summary>
        /// Play the tracks in the playlist
        /// </summary>
        /// <returns></returns>
        public void PlayTracks()
        {
            App.playbackService.StartNewSession(PlaybackSession.PlaybackType.Playlist, tracksHref, tracksCount);
        }

        /// <summary>
        /// Free up memory
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            disposed = true;
            if (disposing)
            {
                image.ClearValue(BitmapImage.UriSourceProperty);
            }
        }
    }
}
