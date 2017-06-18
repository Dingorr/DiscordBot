using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot2.Helpers
{
    class Logger
    {
        private string logPath;
        private string fullFilePath;

        public Logger(string logPath)
        {
            this.logPath = logPath;
            fullFilePath = logPath + DateTime.Now.ToShortDateString() + ".txt";

            if (!Directory.Exists(fullFilePath))
            {
                var fileStream = File.Create(fullFilePath);
                fileStream.Close();
            }
        }

        public async Task LogFileAsync(string info)
        {
            try
            {
                using (var writer = new StreamWriter(File.OpenWrite(fullFilePath)))
                {
                    await writer.WriteLineAsync(LogFormat(info));
                }
            }
            catch (UnauthorizedAccessException e)
            {
                LogConsole($"Error opening log-file for writing {e.Message}", ConsoleColor.Red);
            }
        }

        public void UpdateFilePath()
        {
            fullFilePath = logPath + DateTime.Now.ToShortDateString() + ".txt";
        }

        public static void LogConsole(string info, ConsoleColor bgColor = ConsoleColor.Black, ConsoleColor fgColor = ConsoleColor.White)
        {
            if(bgColor != ConsoleColor.Black)
            {
                Console.BackgroundColor = bgColor;
            }

            if(fgColor != ConsoleColor.White)
            {
                Console.ForegroundColor = fgColor;
            }

            Console.WriteLine(LogFormat(info));
        }

        public static string LogFormat(string info)
        {
            return $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}] - {info}";
        }
    }
}
