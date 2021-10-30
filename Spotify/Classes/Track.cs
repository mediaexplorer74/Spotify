﻿// Track

using Spotify.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Spotify
{
    /// <summary>
    /// A Track object
    /// </summary>
    public class Track : IDisposable
    {
        private bool disposed = false;
        public string id = "";
        public string href = "";
        public string name = "";
        public string albumJson = "";
        public Album album = new Album();
        public List<Artist> artists = new List<Artist>();
        public string previewUrl = "";
        public int duration = 0;

        /// <summary>
        /// The main constructor to create an empty instance
        /// </summary>
        public Track()
        {

        }

        /// <summary>
        /// Get the name of the first Artist
        /// </summary>
        /// <returns>The name of the first Artist</returns>
        public string GetMainArtistName()
        {
            if (this.artists.Count > 0)
            {
                return this.artists.ElementAt(0).name;
            }
            return "";
        }

        /// <summary>
        /// Populate the track information from the JSON object
        /// </summary>
        /// <param name="artistString">The string representation of the track JSON object</param>
        /// <returns></returns>
        public async Task SetInfo(string trackString)
        {
            JsonObject trackJson = new JsonObject();
            try
            {
                trackJson = JsonObject.Parse(trackString);
            }
            catch (COMException)
            {
                return;
            }
            if (trackJson.TryGetValue("track", out IJsonValue trackObject) && trackObject.ValueType == JsonValueType.Object)
            {
                JsonObject trackObjectJson = trackObject.GetObject();
                await SetInfoDirect(trackObjectJson.Stringify());
            }
        }

        /// <summary>
        /// Populate track information from JSON information
        /// </summary>
        /// <param name="trackString">The string representation of the track JSON objects elements</param>
        /// <returns></returns>
        public async Task SetInfoDirect(string trackString)
        {
            JsonObject trackObjectJson = new JsonObject();
            try
            {
                trackObjectJson = JsonObject.Parse(trackString);
            }
            catch (COMException)
            {
                return;
            }
            if (trackObjectJson.TryGetValue("id", out IJsonValue trackId) && trackId.ValueType == JsonValueType.String)
            {
                id = trackId.GetString();
            }
            if (trackObjectJson.TryGetValue("href", out IJsonValue trackHref) && trackHref.ValueType == JsonValueType.String)
            {
                href = trackHref.GetString();
            }
            if (trackObjectJson.TryGetValue("name", out IJsonValue trackName) && trackName.ValueType == JsonValueType.String)
            {
                name = trackName.GetString();
            }
            if (trackObjectJson.TryGetValue("preview_url", out IJsonValue trackPreview) && trackPreview.ValueType == JsonValueType.String)
            {
                previewUrl = trackPreview.GetString();
            }
            if (trackObjectJson.TryGetValue("duration_ms", out IJsonValue trackDuration) && trackDuration.ValueType == JsonValueType.Number)
            {
                duration = Convert.ToInt32(trackDuration.GetNumber());
            }
            if (trackObjectJson.TryGetValue("album", out IJsonValue trackAlbum) && trackAlbum.ValueType == JsonValueType.Object)
            {
                await album.SetInfo(trackAlbum.Stringify());
            }
            if (trackObjectJson.TryGetValue("artists", out IJsonValue trackArtists) && trackArtists.ValueType == JsonValueType.Array)
            {
                JsonArray artistsArray = trackArtists.GetArray();
                foreach (JsonValue artistObject in artistsArray)
                {
                    Artist artist = new Artist();
                    artist.SetInfo(artistObject.Stringify());
                    artists.Add(artist);
                }
            }
        }

        /// <summary>
        /// Play the track
        /// </summary>
        public void PlayTrack()
        {
            App.playbackService.StartNewSession(PlaybackSession.PlaybackType.Single, href, 1);
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

                album.Dispose();
                while (artists.Count > 0)
                {
                    Artist artist = artists.ElementAt(0);
                    artists.Remove(artist);
                    artist.Dispose();
                }
                artists.Clear();
            }
        }
    }
}
