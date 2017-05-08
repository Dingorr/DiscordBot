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

    [Group("memeconomy"), Alias("meco")]
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

        [Command("register"), Summary("Registers user")]
        public async Task Register()
        {
            var userInfo = Context.Message.Author;
            bool succesful = await XmlHelper.RegisterMemeconomyUser(userInfo.Username, userInfo.Id.ToString());
            string reply = succesful ? $"{userInfo.Username} is now registered" : $"{userInfo.Username} is already registered";
            await ReplyAsync(reply);
        }

        [Command("bet")]
        public async Task Bet(string choice, int amount)
        {
            var user = Context.Message.Author;
            var betInfo = await XmlHelper.MemeconomyGetInfoOfBetMeme(choice);
            if(betInfo == null)
            {
                await ReplyAsync($"Choice is invalid");
                return;
            }

            int status = await XmlHelper.MemeconomyBet(user.Id.ToString(), betInfo.FullName, amount);
            if(status == 0)
            {
                await ReplyAsync($"{user.Username} has now betted on meme [{betInfo.Id}] - '{betInfo.PostName}' with {amount} points.");
            }
            else
            {
                switch(status)
                {
                    case 1:
                    case 2:
                        await ReplyAsync("Error in bet parameters.");
                        break;
                    case 3:
                        await ReplyAsync("You do not have enough points to make that bet...");
                        break;
                    case 4:
                        await ReplyAsync("You have already betted today.");
                        break;
                }
            }
        }
    }
}
