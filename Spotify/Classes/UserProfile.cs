﻿// User Profile

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace Spotify.Classes
{
    public static class UserProfile
    {
        public static string displayName = "";
        public static string userId = "";
        public static BitmapImage userPic = new BitmapImage();

        /// <summary>
        /// Updates the users information (name, image, etc)
        /// </summary>
        /// <param name="jsonString">The JSON containing the users information</param>
        /// <returns></returns>
        public async static Task UpdateInfo(string jsonString)
        {
            // parse data
            JsonObject userJson = new JsonObject();
            try
            {
                userJson = JsonObject.Parse(jsonString);
            }
            catch (COMException)
            {
                return;
            }
            if (userJson.TryGetValue("display_name", out IJsonValue displayNameJson) && displayNameJson.ValueType == JsonValueType.String)
            {
                displayName = displayNameJson.GetString();
            }
            if (userJson.TryGetValue("id", out IJsonValue userIdJson) && userIdJson.ValueType == JsonValueType.String)
            {
                userId = userIdJson.GetString();
                if (displayName == "")
                {
                    displayName = userIdJson.GetString();
                }
            }

            // picture
            if (userJson.TryGetValue("images", out IJsonValue imagesJson) && imagesJson.ValueType == JsonValueType.Array)
            {
                JsonArray imageArray = imagesJson.GetArray();
                if (imageArray.Count > 0)
                {
                    JsonObject imageObject = imageArray.ElementAt(0).GetObject();
                    JsonValue urlJson = imageObject.GetNamedValue("url");
                    string url = urlJson.GetString();
                    UriBuilder uri = new UriBuilder(url);

                    BitmapImage bitmapImage = new BitmapImage();
                    HttpClient client = new HttpClient();
                    HttpResponseMessage httpResponse = new HttpResponseMessage();

                    try
                    {
                        httpResponse = await client.GetAsync(uri.Uri);
                        httpResponse.EnsureSuccessStatusCode();
                        IInputStream st = await client.GetInputStreamAsync(uri.Uri);
                        MemoryStream memoryStream = new MemoryStream();
                        await st.AsStreamForRead().CopyToAsync(memoryStream);
                        memoryStream.Position = 0;
                        await bitmapImage.SetSourceAsync(memoryStream.AsRandomAccessStream());
                        userPic = bitmapImage;
                        SaveProfileData();
                    }
                    catch (Exception) { }
                }
            }
        }

        /// <summary>
        /// Save the users information to disk
        /// </summary>
        private static void SaveProfileData()
        {
            //TODO: save user information (name, picture) to disk to limit REST calls
        }

        /// <summary>
        /// Checks whether the user has logged into a Spotify account
        /// </summary>
        /// <returns>True if the user has logged into Spotify, false otherwise</returns>
        public static bool IsLoggedIn()
        {
            if (userId == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
