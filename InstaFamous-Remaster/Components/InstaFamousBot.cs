using System;
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

                return botSettings;
            }
            catch (Exception ex)
            {
                throw ex;
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

                    // Create new instance of reddit and instagram class
                    var redditClient = getRedditClient(botSettings);
                    
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
                            // Log the error to the console
                            Console.WriteLine($"Unable to download {post.Title}, {post.Url}" +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");
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
                            // Log the error to the console
                            Console.WriteLine($"Unable to convert picture format: {file}." +
                                              $" {Environment.NewLine}" +
                                              $" {ex.Message}");
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
                            fileClient.PrepareImages(filePaths);
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

                    var instagramClient = getInstagramClient(botSettings);

                    if (instagramClient.Login())
                    {

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
