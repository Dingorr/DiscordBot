using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot2.Helpers
{
    class Logger
    {
        public static void Log(string info, ConsoleColor bgColor = ConsoleColor.Black, ConsoleColor fgColor = ConsoleColor.White)
        {
            if(bgColor != ConsoleColor.Black)
            {
                Console.BackgroundColor = bgColor;
            }

            if(fgColor != ConsoleColor.White)
            {
                Console.ForegroundColor = fgColor;
            }

            Console.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}] - {info}");
        }
    }
}
