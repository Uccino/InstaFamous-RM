using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaFamous.Components.Reddit
{
    class Post
    {
        private string _url;
        private int _score;
        private string _title;

        public string Url
        {
            get => _url;
            set => _url = value;
        }

        public int Score
        {
            get => _score;
            set =>_score = value;
            
        }

        public string Title
        {
            get => _title;
            set => _title = RegexTitle(value);
        }

        /// <summary>
        /// Cleans the input from anything that is not a character.
        /// </summary>
        /// <param name="inputTitle"></param>
        /// <returns></returns>
        private string RegexTitle(string inputTitle)
        {
            return null;
        }

    }
}
