using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InstaFamous.Components.Reddit
{
    class Post
    {
        private string _title;

        public string Url { get; private set; }
        public int Score { get; private set; }
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
            inputTitle = Regex.Replace(inputTitle, "[^0-9a-zA-Z ]+", "");
            return inputTitle;
        }

    }
}
