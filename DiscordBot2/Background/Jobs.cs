using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using DiscordBot2.Helpers;
using DiscordBot2.Shared;

namespace DiscordBot2.Background
{
    public static class Jobs
    {
        public static async Task RunJobs()
        {
            await RedditTasks.GetMemeOfTheDay();
            await RedditTasks.GetMemeWarOfTheDay();
            await RedditTasks.EndMemeWarOfTheDayAsync();
        }

        public static async Task Initialize()
        {
            await Initializer.Initialize();
        }
    }

    public static class RedditTasks
    {
        public static async Task GetMemeOfTheDay()
        {
            if(DateTime.Now.Hour >= GlobalVariables.StartOfDay)
            {
                string path = GlobalVariables.RedditMemeOfTheDayFullPath;
                var doc = XDocument.Load(path);
                var body = doc.Descendants("body").First();
                var mainNodes = body.Descendants("meme");

                if (mainNodes == null)
                    return;

                if(!mainNodes.Any(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString()))
                {
                    var redditHelper = new RedditHelper();
                    string memeURL = await redditHelper.GetTopMEIRLDayURLAsync();
                    var memeElement = new XElement("meme");
                    memeElement.SetAttributeValue("date", DateTime.Now.ToShortDateString());
                    memeElement.SetAttributeValue("url", memeURL);
                    body.Add(memeElement);
                }

                doc.Save(path);
            }
        }

        public static async Task GetMemeWarOfTheDay()
        {
            string path = GlobalVariables.RedditMemeWarOfTheDayFullPath;
            var doc = XDocument.Load(path);
            var body = doc.Descendants("body").First();
            var mainNodes = body.Descendants("day");

            if (DateTime.Now.Hour >= GlobalVariables.StartOfDay) //Creates initial XML
            {
                if (!mainNodes.Any(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString()))
                {
                    var redditHelper = new RedditHelper();
                    var posts = await redditHelper.GetNewestMemesAsync();

                    var dayElement = new XElement("day");
                    dayElement.SetAttributeValue("date", DateTime.Now.ToShortDateString());
                    int counter = 1;

                    foreach(var post in posts)
                    {
                        var memeElement = new XElement("meme");
                        memeElement.SetAttributeValue("id", counter.ToString());
                        memeElement.SetAttributeValue("fullname", post.FullName);
                        memeElement.SetAttributeValue("name", post.Title);
                        memeElement.SetAttributeValue("url", post.Url.AbsoluteUri);

                        var subEntry = new XElement("entry");
                        subEntry.SetAttributeValue("timestamp", DateTime.Now.ToShortTimeString());
                        subEntry.SetAttributeValue("upvotes", post.Upvotes);
                        subEntry.SetAttributeValue("downvotes", post.Downvotes);
                        subEntry.SetAttributeValue("score", post.Score);
                        memeElement.Add(subEntry);

                        dayElement.Add(memeElement);
                        counter++;
                    }

                    body.Add(dayElement);
                }

                doc.Save(path);
            }

            var currentDayNode = mainNodes.FirstOrDefault(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString());

            if(currentDayNode != null)
            {
                var memeNodes = currentDayNode.Descendants("meme");
                await AddMemeWarOfTheDayEntry(memeNodes, doc);
            }
            else
            {
                //If it is before start of new "day"
                var lastDayNode = mainNodes.FirstOrDefault(x => x.Attribute("date").Value == (DateTime.Now - new TimeSpan(1, 0, 0, 0)).ToShortDateString());

                if(lastDayNode != null)
                {
                    var memeNodes = lastDayNode.Descendants("meme");
                    await AddMemeWarOfTheDayEntry(memeNodes, doc);
                }
            }

        }

        public static async Task EndMemeWarOfTheDayAsync()
        {
            if (DateTime.Now.Hour >= GlobalVariables.MemeWarOfTheDayEnd)
            {
                //Get final results
                string path = GlobalVariables.RedditMemeWarOfTheDayFullPath;

                var doc = XDocument.Load(path);
                var body = doc.Descendants("body").First();
                var dayNode = body.Descendants("day").FirstOrDefault(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString());

                if (dayNode == null)
                    return;

                bool? hasWinner = dayNode?.Descendants("winner").Any();

                if (hasWinner != null && !hasWinner.Value)
                {
                    var memeNodes = dayNode.Descendants("memes").FirstOrDefault().Descendants("meme");
                    var winnerInfo = (id: -1, score: 0, fullname: string.Empty, url: string.Empty, title: string.Empty);

                    foreach(var memeNode in memeNodes)
                    {
                        int score = int.Parse(memeNode.Descendants("entry").LastOrDefault().Attribute("score").Value);

                        if(score > winnerInfo.score)
                        {
                            winnerInfo.score = score;
                            winnerInfo.id = int.Parse(memeNode.Attribute("id").Value);
                            winnerInfo.fullname = memeNode.Attribute("fullname").Value;
                            winnerInfo.url = memeNode.Attribute("url").Value;
                            winnerInfo.title = memeNode.Attribute("title").Value;
                        }
                    }

                    var winnerMeme = new XElement("winner");
                    winnerMeme.SetAttributeValue("id", winnerInfo.id.ToString());
                    winnerMeme.SetAttributeValue("timestamp", DateTime.Now.ToShortTimeString());
                    winnerMeme.SetAttributeValue("fullname", winnerInfo.fullname);
                    winnerMeme.SetAttributeValue("url", winnerInfo.url);
                    winnerMeme.SetAttributeValue("title", winnerInfo.title);
                    dayNode.Add(winnerMeme);
                    doc.Save(path);

                    string userPath = GlobalVariables.RedditMemeconomyUsersFullPath;
                    var userDoc = XDocument.Load(userPath);
                    var userBody = userDoc.Descendants("body").First();
                    var userBetted = userBody.Descendants("user").Where(u => u.Descendants("bet").Any(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString()));

                    foreach(var user in userBetted)
                    {
                        var bet = user.Descendants("bet").FirstOrDefault(x => x.Attribute("date").Value == DateTime.Now.ToShortDateString());
                        string postFullname = bet.Attribute("post").Value;
                        int currentPoints = int.Parse(user.Attribute("points").Value);
                        int bettedPoints = int.Parse(bet.Attribute("points").Value);
                        int pointsDelta = postFullname == winnerInfo.fullname ? bettedPoints : -bettedPoints;
                        currentPoints += pointsDelta;

                        user.SetAttributeValue("points", currentPoints.ToString());
                    }

                    userDoc.Save(userPath);
                }
            }
        }

        private static async Task AddMemeWarOfTheDayEntry(IEnumerable<XElement> nodes, XDocument parent)
        {
            var memeNodes = nodes;
            bool save = false;

            foreach (var memeNode in memeNodes)
            {
                var lastEntry = memeNode.Descendants().Last();
                if (DateTime.Now.Subtract(DateTime.Parse(lastEntry.Attribute("timestamp").Value)).TotalMinutes >= GlobalVariables.RedditMemeWarUpdateRate)
                {
                    save = true;

                    var redditHelper = new RedditHelper();
                    var post = await redditHelper.GetMemeByFullnameAsync(memeNode.Attribute("fullname").Value);

                    var subEntry = new XElement("entry");
                    subEntry.SetAttributeValue("timestamp", DateTime.Now.ToShortTimeString());
                    subEntry.SetAttributeValue("upvotes", post.Upvotes);
                    subEntry.SetAttributeValue("downvotes", post.Downvotes);
                    subEntry.SetAttributeValue("score", post.Score);
                    memeNode.Add(subEntry);
                }
            }

            if (save) parent.Save(GlobalVariables.RedditMemeWarOfTheDayFullPath);
        }
    }

    public static class Initializer
    {
        public static async Task Initialize()
        {
            string redditPath = Directory.GetCurrentDirectory() + GlobalVariables.RedditPath;
            if (!Directory.Exists(redditPath))
            {
                Directory.CreateDirectory(redditPath);
            }

            if(!File.Exists(GlobalVariables.RedditMemeOfTheDayFullPath))
            {
                var doc = new XDocument(new XElement("body"));
                doc.Save(GlobalVariables.RedditMemeOfTheDayFullPath);
            }
            if (!File.Exists(GlobalVariables.RedditMemeWarOfTheDayFullPath))
            {
                var doc = new XDocument(new XElement("body"));
                doc.Save(GlobalVariables.RedditMemeWarOfTheDayFullPath);
            }
            if (!File.Exists(GlobalVariables.RedditMemeconomyUsersFullPath))
            {
                var doc = new XDocument(new XElement("body"));
                doc.Save(GlobalVariables.RedditMemeconomyUsersFullPath);
            }
        }
    }

}
