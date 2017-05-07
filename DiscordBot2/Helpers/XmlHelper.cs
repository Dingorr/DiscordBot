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
            DateTime date = GlobalVariables.CurrentDate;
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
            DateTime date = GlobalVariables.CurrentDate;
            var urls = new List<MemeWarEntry>();
            using (var stream = File.OpenRead(xmlPath))
            {
                var doc = XDocument.Load(stream);
                var dayNode = doc.FirstNode.Document.Descendants("day").First(x => x.Attribute("date").Value == date.ToShortDateString());
                var memeNodes = dayNode.Descendants("meme");

                foreach (var memeNode in memeNodes)
                {
                    string url = memeNode.Attribute("url").Value;
                    string score = memeNode.Descendants("entry").Last().Attribute("score").Value;
                    urls.Add(new MemeWarEntry(url, score));
                }
            }

            return urls.AsEnumerable();
        }

        /// <summary>
        /// Registers user if it doesn't exist i XML file
        /// </summary>
        /// <param name="username">Discord username</param>
        /// <param name="userid">Discord ID</param>
        /// <returns>True if registration is succesful and false if user already exist</returns>
        public async static Task<bool> RegisterMemeconomyUser(string username, string userid)
        {
            string xmlPath = GlobalVariables.RedditMemeconomyUsersFullPath;
            DateTime date = GlobalVariables.CurrentDate;
            bool isRegistered = false;
            bool returnVal = false;

            using (var stream = File.OpenRead(xmlPath))
            {
                var doc = XDocument.Load(stream);
                isRegistered = doc.FirstNode.Document.Descendants("user").Any(x => x.Attribute("id").Value == userid);
            }

            if (!isRegistered)
            {
                var doc = XDocument.Load(xmlPath);
                var body = doc.Descendants("body").First();
                returnVal = true;

                var userNode = new XElement("user");
                userNode.SetAttributeValue("username", username);
                userNode.SetAttributeValue("id", userid);
                userNode.SetAttributeValue("points", GlobalVariables.MemeconomyStartPoints.ToString());
                body.Add(userNode);
                doc.Save(GlobalVariables.RedditMemeconomyUsersFullPath);
            }

            return returnVal;
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
