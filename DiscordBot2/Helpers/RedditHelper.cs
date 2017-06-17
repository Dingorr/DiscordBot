using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedditSharp;
using RedditSharp.Things;
using RedditSharp.Extensions;

namespace DiscordBot2.Helpers
{
    class RedditHelper
    {
        Reddit reddit;
        
        public RedditHelper()
        {
            reddit = new Reddit();
        }

        public async Task<string> GetTopMEIRLDayURLAsync()
        {
            var meirl = await reddit.GetSubredditAsync("/r/me_irl");
            return (await meirl.GetTop(FromTime.Day, 1).First()).Url.AbsoluteUri;
        }

        public async Task<IEnumerable<Post>> GetNewestMemesAsync(int amount = 2)
        {
            var meirl = await reddit.GetSubredditAsync("/r/me_irl");
            
            return meirl.GetPosts(Subreddit.Sort.New, amount).ToEnumerable();
        }

        public async Task<Post> GetMemeByFullnameAsync(string fullname)
        {
            return (Post) await reddit.GetThingByFullnameAsync(fullname);
        }
    }
}
