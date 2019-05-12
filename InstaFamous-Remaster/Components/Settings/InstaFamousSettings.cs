using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;


namespace InstaFamous.Components.Settings
{
    class InstaFamousSettings
    {
        private string _filePath;
      
        public string filePath
        {
            get => _filePath;
            set
            {
                if (File.Exists(value))
                {
                    _filePath = value;
                }
                else
                {
                    throw new FileNotFoundException("Could not find the settings file specified.");
                }
            }
        }

        /// <summary>
        /// Settings manager for the InstaFamous bot
        /// </summary>
        /// <param name="filePath">Filepath to the .JSON file containing the settings for this bot.</param>
        public InstaFamousSettings(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// Attemps to read the settings file specified
        /// </summary>
        /// <returns></returns>
        public BotSettings LoadSettings()
        {
            // Read the JSON settings
            dynamic jsonSettings = ReadJsonFile();

            // Reddit settings
            string subreddit = jsonSettings["Reddit"]["Subreddit"];
            string upvotes = jsonSettings["Reddit"]["UpvoteRequirement"];
            if (!int.TryParse(upvotes, out var upvoteRequirement))
            {
                throw new Exception("Unable set the upvote requirement.");
            }

            // Instagram settings
            string igUsername = jsonSettings["Instagram"]["Username"];
            string igPassword = jsonSettings["Instagram"]["Password"];
            string igTags = jsonSettings["Instagram"]["Tags"];
            
            // Create a new instance of the botsettings
            // Set the settings and return the class
            var botSettings = new BotSettings();

            botSettings.SetRedditSettings(subreddit, upvoteRequirement);
            botSettings.SetInstagramSettings(igUsername, igPassword, igTags);

            return botSettings;
        }

        /// <summary>
        /// Attempts to read the JSON settings file
        /// </summary>
        /// <returns></returns>
        private dynamic ReadJsonFile()
        {
            // Check if the file is there
            if (File.Exists(filePath))
            {
                try
                {
                    // Create a new StreamReader to read the JSON file
                    using (StreamReader jsonReader = new StreamReader(filePath))
                    {
                        // Read the JSON file and convert it to a dynamic
                        dynamic jsonFile = jsonReader.ReadToEnd();
                        var jsonSettings = JsonConvert.DeserializeObject(jsonFile);

                        return jsonSettings;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new FileNotFoundException("Could not find the json file specified");
            }
        }
    }
}
