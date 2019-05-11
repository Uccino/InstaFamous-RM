using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace InstaFamous.Components.Reddit
{
    class RedditClient
    {

        public string Subreddit { get; private set; }
        public int UpvoteThreshold { get; private set; }

        public RedditClient(string subreddit, int upvoteThreshold)
        {
            this.Subreddit = subreddit;
            this.UpvoteThreshold = upvoteThreshold;
        }

        /// <summary>
        /// Gets a list of filtered posts off a subreddit.
        /// </summary>
        /// <returns></returns>
        public List<Post> GetPosts()
        {
            // Get the reddit posts as JSON
            var redditSubmissions = RequestSubmissions();

            // Create a new list of Post classes 
            List<Post> postList = new List<Post>(25);
            // Loop through all of the submissions in the JSON 
            foreach (var post in redditSubmissions)
            {
                // Create a new post instance
                var redditPost = new Post
                {
                    // Set the variables
                    Score = int.Parse(post["data"]["ups"]),
                    Title = post["data"]["title"],
                    Url =  post["data"]["url"]
                };

                // Add it to the list
                postList.Add(redditPost);
            }

            // Filter all of the posts and return it
            var filteredPosts = FilterPosts(postList);
            return filteredPosts;
        }

        /// <summary>
        /// Downloads a reddit post to the bot's working directory
        /// </summary>
        /// <param name="post">Post to download</param>
        /// <param name="directoryName">Directory to download to</param>
        /// <returns></returns>
        public void DownloadPost(Post post, string directoryName)
        {
            // Get information about the post from the class
            var postName = post.Title;
            var postUrl = post.Url;
            var filePath = directoryName + "\\" + postName;

            // Add .png or .jpg depending on what is in the url
            filePath += postUrl.Contains(".png") ? ".png" : ".jpg";

            // Attempt to download the post
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(postUrl, filePath);
            }
        }

        /// <summary>
        /// Builds the uri for a reddit request
        /// </summary>
        /// <param name="subreddit">Subreddit to build the uri for</param>
        /// <returns></returns>
        private Uri BuildUrl(string subreddit)
        {
            string url = $"https://reddit.com/r/{subreddit}/hot.json?count=50";
            Uri redditUrl = new Uri(url);
            return redditUrl;
        }

        /// <summary>
        /// Create a request for the top 25 reddit posts of a subreddit.
        /// </summary>
        /// <returns></returns>
        private dynamic RequestSubmissions()
        {
            HttpWebResponse webResponse = null;

            // Attempt to get the reddit posts
            var redditUrl = BuildUrl(Subreddit);
            try
            {
                // Create a webrequest
                var redditRequest = WebRequest.Create(redditUrl);
                var redditResponse = (HttpWebResponse) redditRequest.GetResponse();
                if (redditResponse.StatusCode == HttpStatusCode.OK)
                {
                    webResponse = redditResponse;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Attempt to read the data from the response
            try
            {
                // Get the response stream
                using (var dataStream = webResponse.GetResponseStream())
                {
                    // Read the stream data
                    var dataReader = new StreamReader(dataStream);
                    var serverResponse = dataReader.ReadToEnd();

                    // Parse the json data
                    dynamic redditPosts = JsonConvert.DeserializeObject(serverResponse);

                    // Return the posts
                    return redditPosts["data"]["children"];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Filters the post based on the upvote requirement
        /// </summary>
        /// <param name="redditPosts">List of reddit posts</param>
        /// <returns></returns>
        private List<Post> FilterPosts(List<Post> redditPosts)
        {
            var filteredList = new List<Post>();

            foreach (var post in redditPosts)
            {
                // Only add the posts that have a certain amount of upvotes
                if (post.Score >= UpvoteThreshold)
                {
                    filteredList.Add(post);
                }
            }

            return filteredList;
        }

    }
}
