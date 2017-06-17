using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DiscordBot2.Background;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot2
{
    class Program
    {
        private const int TimerRefreshRate = 30000;

        private CommandService commands;
        private DiscordSocketClient client;
        private IServiceProvider services;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Task initialize = Jobs.Initialize();
            await Jobs.RunJobs();

            client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            commands = new CommandService();
            services = new ServiceCollection()
                    .BuildServiceProvider();

            client.Log += Log;
            //client.MessageReceived += MessageReceived;

            await InstallCommands();

            string token = "MzA4NjY3ODA3NjQwNDUzMTMw.C-kNVA.zMF69NMczApp_8iGOlYCBOqfdF4";
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await initialize;
            var timer = new Timer(TimerRefreshRate);
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Jobs.RunJobs().GetAwaiter().GetResult();
        }

        private Task Log(LogMessage msg)
        {
            var cc = Console.ForegroundColor;
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
            }
            Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message}");
            Console.ForegroundColor = cc;
            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content == "!ping")
            {
                await message.Channel.SendMessageAsync("Pong!");
            }
        }

        public async Task InstallCommands()
        {
            // Hook the MessageReceived Event into our Command Handler
            client.MessageReceived += HandleCommand;
            await commands.AddModuleAsync<MOTD>();
            await commands.AddModuleAsync<Memeconomy>();
        }

        public async Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new CommandContext(client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed succesfully)
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
