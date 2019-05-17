using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaFamous.Components.Settings
{
    class BotSettings
    {
        // Reddit settings
        public string Subreddit { get; private set; }
        public int UpvoteThreshold { get; private set; }

        // Instagram settings
        public string InstagramUsername { get; private set; }
        public string InstagramPassword { get; private set; }
        public string InstagramTags { get; private set; }
        
        /// <summary>
        /// Sets the reddit settings
        /// </summary>
        /// <param name="subreddit">Subreddit to browse</param>
        /// <param name="upvoteThreshold">Upvotethreshold used</param>
        public void SetRedditSettings(string subreddit, int upvoteThreshold)
        {
            Subreddit = subreddit;
            UpvoteThreshold = upvoteThreshold;
        }
        /// <summary>
        /// Sets the instagram settings
        /// </summary>
        /// <param name="username">Instagram username</param>
        /// <param name="password">Instagram password</param>
        /// <param name="tags">Tags to use with each picture</param>
        public void SetInstagramSettings(string username, string password, string tags)
        {
            InstagramUsername = username;
            InstagramPassword = password;
            InstagramTags = tags;
        }
    }
}
