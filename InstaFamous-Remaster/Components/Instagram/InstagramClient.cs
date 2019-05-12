using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using InstagramApiSharp.API;
using InstagramApiSharp.API.Builder;
using InstagramApiSharp.Classes;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Logger;


namespace InstaFamous.Components.Instagram
{
    class InstagramClient
    {
        private IInstaApi InstaClient;
        private string instagramTags;

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
                .UseLogger(new DebugLogger(LogLevel.Exceptions))
                .SetRequestDelay(delay)
                .Build();

        }

        public bool Login()
        {
            var loginResult = Task.Run(() => InstaClient.LoginAsync().Result);
            if (loginResult.Result.Succeeded && InstaClient.IsUserAuthenticated)
            {
                return true;
            }

            return false;
        }

        public bool Logout()
        {
            if (!InstaClient.IsUserAuthenticated)
            {
                var logoutResult = Task.Run(() => InstaClient.LogoutAsync().Result);
                if (logoutResult.Result.Succeeded && !InstaClient.IsUserAuthenticated)
                {
                    return true;
                }
            }

            return false;
        }



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
                ImageBytes = GetImageBytes(filePath),
                Uri = filePath
            };

            if (InstaClient.IsUserAuthenticated)
            {
                var uploadResult = Task.Run(async () => await InstaClient.MediaProcessor.UploadPhotoAsync(mediaImage, instagramCaption));
                Console.WriteLine(uploadResult.Result.Info);
                igImage.Dispose();
                if (uploadResult.Result.Succeeded)
                {
                    igImage.Dispose();
                    return true;
                }

            }
            igImage.Dispose();
            return false;
        }

        private byte[] GetImageBytes(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Create a byte array of file stream length
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                //Read block of bytes from stream into the byte array
                fs.Read(bytes, 0, System.Convert.ToInt32(fs.Length));
                //Close the File Stream
                fs.Close();
                return bytes; //return the byte data
            }
        }
    }
}
