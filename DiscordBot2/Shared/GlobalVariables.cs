using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot2.Shared
{
    public static class GlobalVariables
    {
        public const string RedditPath = "/Reddit";
        public const string RedditMemeOfTheDayXml = "/MemeOfTheDay.xml";
        public const string RedditMemeWarOfTheDayXml = "/MemeWarOfTheDay.xml";
        public const int StartOfDay = 6;

        public const int RedditMemeWarUpdateRate = 5;

        public static string RedditMemeOfTheDayFullPath => Directory.GetCurrentDirectory() + RedditPath + RedditMemeOfTheDayXml;
        public static string RedditMemeWarOfTheDayFullPath => Directory.GetCurrentDirectory() + RedditPath + RedditMemeWarOfTheDayXml;
    }
}
