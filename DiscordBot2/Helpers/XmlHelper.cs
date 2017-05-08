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

        /// <summary>
        /// Places a bet in the XML with the provided info, if possible
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="postFullname"></param>
        /// <param name="amount"></param>
        /// <returns>
        /// 0 -> Success
        /// 1 -> Error parsing user XML or MWOTD XML
        /// 2 -> betted points are invalid
        /// 3 -> User has no more points
        /// 4 -> User has already betted today
        /// </returns>
        public async static Task<int> MemeconomyBet(string userId, string postFullname, int amount)
        {
            string MWOTDXmlPath = GlobalVariables.RedditMemeWarOfTheDayFullPath;
            string MWOTDUserXmlPath = GlobalVariables.RedditMemeconomyUsersFullPath;
            DateTime date = GlobalVariables.CurrentDate;
            XElement memeNode = null;
            XElement userNode = null;
            int userPoints = -1;

            var memeNodeTask = Task.Run(() =>
            {
                using (var stream = File.OpenRead(MWOTDXmlPath))
                {
                    var doc = XDocument.Load(stream);
                    var dayNode = doc.FirstNode.Document.Descendants("day").First(x => x.Attribute("date").Value == date.ToShortDateString());
                    memeNode = dayNode.Descendants("meme").Where(x => x.Attribute("fullname").Value == postFullname).FirstOrDefault();

                    if (memeNode == null)
                    {
                        return false;
                    }
                    return true;
                }
            });

            var userNodeTask = Task.Run(() =>
            {
                using (var stream = File.OpenRead(MWOTDUserXmlPath))
                {
                    var body = XDocument.Load(stream).Descendants("body").First();
                    userNode = body.Descendants("user").Where(x => x.Attribute("id").Value == userId).FirstOrDefault();

                    if (userNode == null)
                    {
                        return false;
                    }
                    return true;
                }
            });

            bool memeNodeTaskSuccess = await memeNodeTask;
            bool userNodeTaskSuccess = await userNodeTask;

            if(!memeNodeTaskSuccess || !userNodeTaskSuccess)
                return 1;
            
            bool userCastSuccess = int.TryParse(userNode.Attribute("points").Value, out userPoints);
            if (!userCastSuccess)
                return 2;

            if (userPoints <= 0)
                return 3;

            bool hasBettedToday = userNode.Descendants("bet").Any(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString());
            if (hasBettedToday)
                return 4;

            //Create bet
            var bet = new XElement("bet");
            bet.SetAttributeValue("date", DateTime.Now.ToShortDateString());
            bet.SetAttributeValue("post", postFullname);
            bet.SetAttributeValue("points", amount.ToString());
            userNode.SetAttributeValue("points", (userPoints - amount).ToString());
            userNode.Add(bet);

            var saveDoc = XDocument.Load(MWOTDUserXmlPath);
            saveDoc.Descendants("body").First().Descendants("user").First(x => x.Attribute("id").Value == userId).ReplaceWith(userNode);
            saveDoc.Save(MWOTDUserXmlPath);

            return 0;
        }

        public static async Task<MemeWarMeme> MemeconomyGetInfoOfBetMeme(string bet)
        {
            string xmlPath = GlobalVariables.RedditMemeWarOfTheDayFullPath;
            DateTime date = GlobalVariables.CurrentDate;
            int choiceId = -1;
            bool canPassChoice = int.TryParse(bet, out choiceId);
            string id = string.Empty;
            string fullName = string.Empty;
            string postName = string.Empty;

            if (!canPassChoice)
                return null;

            var memeNodeTask = Task.Run(() =>
            {
                using (var stream = File.OpenRead(xmlPath))
                {
                    var doc = XDocument.Load(stream);
                    var dayNode = doc.FirstNode.Document.Descendants("day").First(x => x.Attribute("date").Value == date.ToShortDateString());
                    var memeNode = dayNode.Descendants("meme").FirstOrDefault(x => x.Attribute("id").Value == choiceId.ToString());

                    if (memeNode == null)
                        return false;

                    id = memeNode.Attribute("id").Value;
                    fullName = memeNode.Attribute("fullname").Value;
                    postName = memeNode.Attribute("name").Value;

                    return true;
                }
            });

            bool memeNodeTaskSuccess = await memeNodeTask;
            return new MemeWarMeme(id, fullName, postName);
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

    class MemeWarMeme
    {
        public string Id { get; private set; }
        public string FullName { get; private set; }
        public string PostName { get; private set; }

        public MemeWarMeme(string id, string fullName, string postName)
        {
            Id = id;
            FullName = fullName;
            PostName = postName;
        }
    }
}
