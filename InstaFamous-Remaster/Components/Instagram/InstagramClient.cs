using System;
using System.Drawing;
using System.IO;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;

namespace InstaFamous.Components.Instagram
{
    class InstagramClient
    {
        private readonly IInstaApi InstaClient;
        private readonly string instagramTags;

        public InstagramClient(string username, string password, string tags)
        {
            instagramTags = tags;
            // Get the new usersession
            var userSession = new UserSessionData()
            {
                UserName = username,
                Password = password
            };

            // Build the instagramclient
            var delay = RequestDelay.FromSeconds(1, 3);
            InstaClient = InstaApiBuilder.CreateBuilder()
                .SetUser(userSession)
                .UseLogger(new DebugLogger(LogLevel.None))
                .SetRequestDelay(delay)
                .Build();

        }

        /// <summary>
        /// Logs the user in to instagram
        /// </summary>
        /// <returns></returns>
        public bool Login()
        {
            var loginResult = InstaClient.LoginAsync().Result;

            return loginResult.Succeeded;
        }

        /// <summary>
        /// Logout the user from instagram
        /// </summary>
        /// <returns></returns>
        public bool Logout()
        {
            if (InstaClient.IsUserAuthenticated)
            {
                var logoutResult = InstaClient.LogoutAsync().Result;
                return logoutResult.Succeeded;
            }

            return true;

        }
        
        /// <summary>
        /// Attempts to upload an image to instagram
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool PostImage(string filePath)
        {
            var captionTags = instagramTags;
            var captionTitle = Path.GetFileNameWithoutExtension(filePath);
            var instagramCaption = captionTitle + Environment.NewLine + captionTags;

            var igImage = Image.FromFile(filePath);

            var mediaImage = new InstaImageUpload()
            {
                Height = igImage.Height,
                Width = igImage.Width,
                ImageBytes = File.ReadAllBytes(filePath),
                Uri = filePath
            };

            if (InstaClient.IsUserAuthenticated)
            {
                var uploadResult = InstaClient.MediaProcessor.UploadPhotoAsync(mediaImage, instagramCaption).Result;
                igImage.Dispose();
                if (uploadResult.Succeeded)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
