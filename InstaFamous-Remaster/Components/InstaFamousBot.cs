using System;
using System.IO;
using InstaFamous.Components.Instagram;
using InstaFamous.Components.Reddit;
using InstaFamous.Components.Settings;
using InstaFamous.Components.FileHandler;

namespace InstaFamous.Components
{
    class InstaFamousBot
    {
        public string BotName { get; private set; }
        public int ImagesUploaded { get; private set; }
        public int ImagesDownloaded { get; private set; }


        public BotSettings Setup(string settingsPath)
        {
            try
            {
                var settingsManager = new InstaFamousSettings(settingsPath);
                var botSettings = settingsManager.LoadSettings();

                BotName = botSettings.Subreddit + " | " + botSettings.InstagramUsername;

                InstaFamousLogger.LogMessage($"Set up bot for ${BotName}",
                    InstaFamousLogger.LogLevel.INFO,
                    BotName);

                return botSettings;

            }
            catch (Exception ex)
            {
                InstaFamousLogger.LogMessage($"Unable to setup bot for: ${BotName}" +
                                             $" {Environment.NewLine}" +
                                             $" {ex.Message}",
                    InstaFamousLogger.LogLevel.WARNING, BotName);
                return null;
            }
        }

        public void Run(BotSettings settings)
        {
            ImagesDownloaded = 0;
            ImagesUploaded = 0;

            var botSettings = settings;

            // Attempt to create a new directory
            string directoryName = settings.Subreddit;

            var fileClient = new FileManager(directoryName);

            if (!fileClient.Setup())
            {
                // Main bot loop
                while (true)
                {
                    // Here we download all the reddit images and prepare them for uploading

                    // Create new instance of reddit class
                    var redditClient = getRedditClient(botSettings);

                    InstaFamousLogger.LogMessage("Downloading new reddit posts", InstaFamousLogger.LogLevel.INFO, BotName);
                    // Try to get the new reddit post
                    var redditPosts = redditClient.GetPosts();

                    // Loop over the list and download the reddit posts
                    foreach (var post in redditPosts)
                    {
                        try
                        {
                            redditClient.DownloadPost(post, directoryName);
                            ImagesDownloaded += 1;
                        }
                        catch (Exception ex)
                        {
                            InstaFamousLogger.LogMessage($"Unable to download {post.Url} " +
                                                         $"{Environment.NewLine}" +
                                                         $" {ex.Message}",
                                InstaFamousLogger.LogLevel.WARNING, BotName);
                        }
                    }

                    // Get all of the items in the directory and convert the file types to jpg
                    var pngFilePaths = fileClient.GetPngImages();
                    foreach (var file in pngFilePaths)
                    {
                        try
                        {
                            fileClient.ChangePictureFormat(file);
                        }
                        catch (Exception ex)
                        {
                            InstaFamousLogger.LogMessage($"Unable to change the picture format of {file} " +
                                                         $"{Environment.NewLine}" +
                                                         $" {ex.Message}",
                                InstaFamousLogger.LogLevel.WARNING, BotName);
                        }

                        // Remove the old file
                        System.IO.File.Delete(file);
                    }

                    // Prepare the images for uploading
                    // Remove EXIF
                    // Resize images and add padding.
                    var filePaths = fileClient.GetImageList();
                    foreach (var file in filePaths)
                    {
                        try
                        {
                            fileClient.PrepareImage(file);
                        }
                        catch (Exception ex)
                        {
                            // Log the error to the console
                            Console.WriteLine($"Unable to prepare image: {file}" +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");
                        }
                    }

                    // Here we upload the images to instagram.

                    // Get new instagram class instance
                    var instagramClient = getInstagramClient(botSettings);

                    //// Loop through the images and upload them one by one
                    var instagramFiles = fileClient.GetImageList();
                    foreach (var filePath in instagramFiles)
                    {
                        // Attempt to login
                        try
                        {
                            instagramClient.Login();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to login to instagram account {botSettings.InstagramUsername}" +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");
                        }
                        // Attempt to upload the image to instagram.
                        try
                        {
                            instagramClient.PostImage(filePath);
                            Console.WriteLine($"Successfully uploaded {filePath}" +
                                              $" {Environment.NewLine}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to upload {filePath}" +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");

                        }

                        // Attempt to logout
                        try
                        {
                            instagramClient.Logout();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to logout to instagram account {botSettings.InstagramUsername}" +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");
                        }

                        System.Threading.Thread.Sleep(216 * 100000);
                    }

                    // Clean up the directory of images
                    var images = Directory.EnumerateFiles(directoryName);
                    foreach (var image in images)
                    {
                        try
                        {
                            File.Delete(image);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Unable to delete file {image}" +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new instance of the reddit client class
        /// </summary>
        /// <param name="redditSettings"></param>
        /// <returns></returns>
        private RedditClient getRedditClient(BotSettings redditSettings)
        {
            // Get the settings
            var subreddit = redditSettings.Subreddit;
            var upvoteThreshold = redditSettings.UpvoteThreshold;

            // Create new instance of the class
            var redditClient = new RedditClient(subreddit, upvoteThreshold);

            return redditClient;
        }

        /// <summary>
        /// Creates a new instance of the instagram client class
        /// </summary>
        /// <param name="instagramSettings"></param>
        /// <returns></returns>
        private InstagramClient getInstagramClient(BotSettings instagramSettings)
        {
            // Get the settings
            var igUsername = instagramSettings.InstagramUsername;
            var igPassword = instagramSettings.InstagramPassword;
            var igTags = instagramSettings.InstagramTags;

            // Create new instance of the class
            var igClient = new InstagramClient(igUsername, igPassword, igTags);

            return igClient;
        }
    }
}
