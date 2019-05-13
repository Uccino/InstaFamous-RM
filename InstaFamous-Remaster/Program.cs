using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InstaFamous.Components;
using InstaFamous.Components.Settings;

namespace InstaFamous
{
    class Program
    {
        static void Main(string[] args)
        {
            var savedSettings = new List<string>();

            while (true)
            {
                var settings = Directory.EnumerateFiles("./Settings");
                foreach (var setting in settings)
                {
                    if (!savedSettings.Contains(setting))
                    {
                        var newBot = new InstaFamousBot();

                        savedSettings.Add(setting);
                        Thread newThread = new Thread(()=> newBot.Start(setting));
                        newThread.Start();
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
