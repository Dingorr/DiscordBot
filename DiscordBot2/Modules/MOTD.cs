using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using DiscordBot2.Helpers;
using DiscordBot2.Shared;
using System.IO;
using System.Xml.Linq;

namespace DiscordBot2
{
    class MOTD : ModuleBase
    {
        [Command("motd"), Summary("Meme of the Day")]
        public async Task Memeoftheday()
        {
            var url = await XmlHelper.GetRedditMemeOfTheDayAsync();
            await ReplyAsync(url);
        }
    }

    // Create a module with the 'sample' prefix
    [Group("memeconomy")]
    public class Memeconomy : ModuleBase
    {
        // ~sample square 20 -> 400
        [Command("memes"), Summary("List memes in todays meme war")]
        public async Task Memes()
        {
            var entries = await XmlHelper.GetRedditMemeWarEntriesAsync();
            var stringBuilder = new StringBuilder();

            int counter = 1;

            stringBuilder.AppendLine("Memes in the war today:");
            foreach(var entry in entries)
            {
                stringBuilder.AppendLine($"[{counter}] {entry.URL} - {entry.Score}");
                counter++;
            }

            await ReplyAsync(stringBuilder.ToString());
        }

        [Command("userinfo"), Summary("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfo([Summary("The (optional) user to get info for")] IUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }
}
