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
        public const string RedditMemeconomyUsersXml = "/MemeconomyUsersXml.xml";
        public const string LogPath = "/Logs";
        public const int StartOfDay = 6;
        public const int MemeWarOfTheDayEnd = 20;
        public const int MemeconomyStartPoints = 100;

        public const int RedditMemeWarUpdateRate = 5;

        public static string RedditMemeOfTheDayFullPath => Directory.GetCurrentDirectory() + RedditPath + RedditMemeOfTheDayXml;
        public static string RedditMemeWarOfTheDayFullPath => Directory.GetCurrentDirectory() + RedditPath + RedditMemeWarOfTheDayXml;
        public static string RedditMemeconomyUsersFullPath => Directory.GetCurrentDirectory() + RedditPath + RedditMemeconomyUsersXml;
        public static string LogFullPath => Directory.GetCurrentDirectory() + LogPath;
        public static DateTime CurrentDate => DateTime.Now.Hour >= StartOfDay ? DateTime.Now.Date : DateTime.Now.Date.Subtract(TimeSpan.FromDays(1));
    }
}
