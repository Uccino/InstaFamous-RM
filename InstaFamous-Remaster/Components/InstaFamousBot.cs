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

            if (fileClient.Setup())
            {
                // Main bot loop
                while (true)
                {
                    // Create new instance of reddit class
                    var redditClient = GetRedditClient(botSettings);
                    var instagramClient = GetInstagramClient(botSettings);
                    
                    // Try to get the new reddit post
                    var redditPosts = redditClient.GetPosts();
                    redditPosts.ForEach(post => { DownloadFile(redditClient, post, directoryName); });

                    // Get all of the items in the directory and convert the file types to jpg
                    var pngFilePaths = fileClient.GetPngImages();
                    pngFilePaths.ForEach(file => { ChangePictureFormat(fileClient, file); });
                   
                    // Prepare the images for uploading
                    var filePaths = fileClient.GetImageList();
                    filePaths.ForEach(file => { PrepareImage(fileClient, file); });
                    
                    // Loop through the images and upload them one by one
                    var instagramFiles = fileClient.GetImageList();
                    foreach (var filePath in instagramFiles)
                    {
                        // Login to instagram
                        if (LoginInstagram(instagramClient))
                        {
                            // Attempt to upload the image to instagram.
                            try
                            {
                                instagramClient.PostImage(filePath);
                                InstaFamousLogger.LogMessage($"Succesfully uploaded {filePath}",
                                    InstaFamousLogger.LogLevel.INFO,
                                    BotName);
                            }
                            catch (Exception ex)
                            {
                                InstaFamousLogger.LogMessage($"Unable to upload {filePath}" +
                                                             $" {Environment.NewLine}" +
                                                             $" {ex.Message}",
                                    InstaFamousLogger.LogLevel.INFO, 
                                    BotName );
                            }
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

                        // Sleep 6 hours
                        System.Threading.Thread.Sleep(216 * 100000);
                    }

                    // Clean up the directory of images
                    EmptyDirectory(directoryName);
                }
            }
        }

        


        /// <summary>
        /// Creates a new instance of the reddit client class
        /// </summary>
        /// <param name="redditSettings"></param>
        /// <returns></returns>
        private RedditClient GetRedditClient(BotSettings redditSettings)
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
        private InstagramClient GetInstagramClient(BotSettings instagramSettings)
        {
            // Get the settings
            var igUsername = instagramSettings.InstagramUsername;
            var igPassword = instagramSettings.InstagramPassword;
            var igTags = instagramSettings.InstagramTags;

            // Create new instance of the class
            var igClient = new InstagramClient(igUsername, igPassword, igTags);

            return igClient;
        }

        /// <summary>
        /// Attempts to download the reddit post
        /// </summary>
        /// <param name="redditClient"></param>
        /// <param name="redditPost"></param>
        /// <param name="downloadDirectory"></param>
        private void DownloadFile(RedditClient redditClient, Post redditPost, string downloadDirectory)
        {
            try
            {
                // Attempt to download the message
                redditClient.DownloadPost(redditPost, downloadDirectory);
            }
            catch (Exception ex)
            {
                InstaFamousLogger.LogMessage($"Unable to download {redditPost.Url} " +
                                             $"{Environment.NewLine}" +
                                             $" {ex.Message}",
                    InstaFamousLogger.LogLevel.WARNING, BotName);
            }
        }

        /// <summary>
        /// Attempts to change the picture format of a PNG file
        /// </summary>
        /// <param name="fileClient"></param>
        /// <param name="file"></param>
        private void ChangePictureFormat(FileManager fileClient, string file)
        {
            try
            {

                fileClient.ChangePictureFormat(file);
                File.Delete(file);

            }
            catch (Exception ex)
            {
                InstaFamousLogger.LogMessage($"Unable to change the picture format of {file} " +
                                             $"{Environment.NewLine}" +
                                             $" {ex.Message}",
                    InstaFamousLogger.LogLevel.WARNING, BotName);
            }
        }

        /// <summary>
        /// Attempts to prepare the image for uploading
        /// </summary>
        /// <param name="fileClient"></param>
        /// <param name="file"></param>
        private void PrepareImage(FileManager fileClient, string file)
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

        /// <summary>
        /// Clears the working directory
        /// </summary>
        /// <param name="directoryName"></param>
        private void EmptyDirectory(string directoryName)
        {
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

        /// <summary>
        /// Attempts to login to instagram
        /// </summary>
        /// <param name="instagramClient"></param>
        /// <returns></returns>
        private bool LoginInstagram(InstagramClient instagramClient)
        {
            // Attempt to login
            try
            {
                instagramClient.Login();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to login to instagram account of {BotName}" +
                                  $" {Environment.NewLine}" +
                                  $" {ex.Message}");
                return false;
            }
        }
    }
}
