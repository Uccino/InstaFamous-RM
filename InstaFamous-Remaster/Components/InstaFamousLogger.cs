using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstaFamous.Components
{
    static class InstaFamousLogger
    {
        public enum LogLevel { INFO, DEBUG, WARNING}

        public static void LogMessage(string message, LogLevel level, string botName)
        {
            ConsoleColor color;
            string logMessage = $"{DateTime.Now.ToShortTimeString()} | {botName} | {message}";
            switch (level)
            {
                case LogLevel.INFO:
                    color = ConsoleColor.Green;
                    break;
                case LogLevel.DEBUG:
                    color = ConsoleColor.Yellow;
                    break;
                case LogLevel.WARNING:
                    WriteToLog(logMessage);
                    color = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            Console.ForegroundColor = color;
            Console.WriteLine(logMessage);
            Console.ResetColor();
        }
        private static void WriteToLog(string logMessage)
        {
            string logFileName = DateTime.Now.ToShortDateString() + " log.txt"; 
            if (!File.Exists(logFileName))
            {
                File.Create(logFileName);
            }

            using (StreamWriter sWriter = new StreamWriter(logFileName, true))
            {
                sWriter.WriteLine(logMessage);
            }
        }
    }
}
