using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot2.Shared;
using System.IO;
using System.Xml.Linq;

namespace DiscordBot2.Helpers
{
    static class XmlHelper
    {
        public async static Task<string> GetRedditMemeOfTheDayAsync()
        {
            string xmlPath = GlobalVariables.RedditMemeOfTheDayFullPath;
            DateTime date = DateTime.Now.Hour >= GlobalVariables.StartOfDay ? DateTime.Now.Date : DateTime.Now.Date.Subtract(TimeSpan.FromDays(1));
            string url = string.Empty;
            using (var stream = File.OpenRead(xmlPath))
            {
                var doc = XDocument.Load(stream);
                url = doc.FirstNode.Document.Descendants("meme").First(x => x.Attribute("date").Value == date.ToShortDateString()).Attribute("url").Value;
            }

            return url;
        }

        public async static Task<IEnumerable<MemeWarEntry>> GetRedditMemeWarEntriesAsync()
        {
            string xmlPath = GlobalVariables.RedditMemeWarOfTheDayFullPath;
            DateTime date = DateTime.Now.Hour >= GlobalVariables.StartOfDay ? DateTime.Now.Date : DateTime.Now.Date.Subtract(TimeSpan.FromDays(1));
            var urls = new List<MemeWarEntry>();
            using (var stream = File.OpenRead(xmlPath))
            {
                var doc = XDocument.Load(stream);
                var dayNode = doc.FirstNode.Document.Descendants("day").First(x => x.Attribute("date").Value == date.ToShortDateString());
                var memeNodes = dayNode.Descendants("meme");

                foreach(var memeNode in memeNodes)
                {
                    string url = memeNode.Attribute("url").Value;
                    string score = memeNode.Descendants("entry").Last().Attribute("score").Value;
                    urls.Add(new MemeWarEntry(url, score));
                }
            }

            return urls.AsEnumerable();
        }
    }

    struct MemeWarEntry
    {
        public string URL { get; private set; }
        public string Score { get; private set; }

        public MemeWarEntry(string url, string score)
        {
            URL = url;
            Score = score;
        }
    }
}
