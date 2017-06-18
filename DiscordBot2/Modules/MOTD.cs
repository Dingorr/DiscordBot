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
        public CommandService Commands { get; set; }

        [Command]
        public async Task Default()
        {
            var commands = Commands.Modules.FirstOrDefault(x => x.Name.ToLowerInvariant() == "memeconomy").Commands.Where(x => x.Name.ToLowerInvariant() != "default");
            var cmdReply = new StringBuilder();
            string cmdPrefix = "!memeconomy";
            cmdReply.AppendLine("Available commands are:");

            foreach(var command in commands)
            {
                cmdReply.AppendLine($"- \"{cmdPrefix} {command.Name}\" [{command.Summary}]");
            }

            await ReplyAsync(cmdReply.ToString());
        }

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

        [Command("register"), Summary("Register for betting")]
        public async Task Register()
        {
            var userInfo = Context.Message.Author;
            bool succesful = await XmlHelper.RegisterMemeconomyUserAsync(userInfo.Username, userInfo.Id.ToString());
            string reply = succesful ? $"{userInfo.Username} is now registered" : $"{userInfo.Username} is already registered";
            await ReplyAsync(reply);
        }

        [Command("bet"), Summary("Bet on meme of the day")]
        public async Task Bet(string choice, int amount)
        {
            var user = Context.Message.Author;
            var betInfo = await XmlHelper.MemeconomyGetInfoOfBetMemeAsync(choice);
            if(betInfo == null)
            {
                await ReplyAsync($"Choice is invalid");
                return;
            }

            int status = await XmlHelper.MemeconomyBetAsync(user.Id.ToString(), betInfo.FullName, amount);
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

        [Command("winner"), Summary("Shows the meme which has won today")]
        public async Task Winner()
        {
            var winner = await XmlHelper.MemeconomyGetWinnerAsync();

            if(winner != null)
            {
                await ReplyAsync($"Winning meme of today is {winner.PostName} ({winner.Id}) - {winner.Url} with a total score of {winner.Score}");
            }
            else
            {
                await ReplyAsync($"No winner yet. Winner will be chosen at {GlobalVariables.MemeWarOfTheDayEnd} o'clock.");
            }
        }

        [Command("points"), Summary("Shows how many points you have")]
        public async Task Points()
        {
            var user = Context.Message.Author;
            string points = await XmlHelper.MemeconomyGetPointsAsync(user.Id.ToString());

            if(!string.IsNullOrWhiteSpace(points))
            {
                await ReplyAsync($"You have {points} points!");
            }
            else
            {
                await ReplyAsync($"You aren't registered yet. Type \"!memeconomy register\" to register");
            }
        }

        [Command("restart"), Summary("Resets your Memeconomy user")]
        public async Task RestartUser()
        {
            await XmlHelper.ResetMemeconomyUserAsync(Context.Message.Author.Username, Context.Message.Author.Id.ToString());
            await ReplyAsync("You have now resetted your user");
        }
    }
}
