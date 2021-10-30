// Artist

using System;
using System.Runtime.InteropServices;
using Windows.Data.Json;

namespace Spotify
{
    /// <summary>
    /// An Artist object
    /// </summary>
    public class Artist : IDisposable
    {
        private bool disposed = false;
        public string name = "";

        /// <summary>
        /// Populate the artist information from the JSON object
        /// </summary>
        /// <param name="artistString">The string representation of the artist JSON object</param>
        public void SetInfo(string artistString)
        {
            JsonObject trackJson = new JsonObject();
            try
            {
                trackJson = JsonObject.Parse(artistString);
            }
            catch (COMException)
            {
                return;
            }
            if (trackJson.TryGetValue("name", out IJsonValue nameJson) && nameJson.ValueType == JsonValueType.String)
            {
                name = nameJson.GetString();
            }
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
               
            }
        }
    }
}
