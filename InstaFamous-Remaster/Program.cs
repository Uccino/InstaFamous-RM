using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstaFamous.Components;
using InstaFamous.Components.Settings;

namespace InstaFamous
{
    class Program
    {
        static void Main(string[] args)
        {

            string filePath = "./Settings/Dankmemes.json";

            InstaFamousBot ifBot = new InstaFamousBot();
            var botSettings = ifBot.Setup(filePath);
            if (botSettings != null)
            {
                ifBot.Run(botSettings);
            }
        }
    }
}
