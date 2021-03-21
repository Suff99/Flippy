using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Discord
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().RunBotAsASync().GetAwaiter().GetResult();
        }


        private DiscordSocketClient client;
        private CommandService commandService;
        private IServiceProvider services;

        public async Task RunBotAsASync()
        {
            client = new DiscordSocketClient();
            commandService = new CommandService();
            services = new ServiceCollection().AddSingleton(client).AddSingleton(commandService).BuildServiceProvider();

            string token = GetToken();
            client.Log += logger;
            await RegisterCommands();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task logger(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }


        public string GetToken() {
            string tokenFile = @"./config/config.txt";

            if (File.Exists(tokenFile)) {
                string[] lines = System.IO.File.ReadAllLines(@"./config/config.txt");

                if (lines.Length < 0) {
                    Console.WriteLine("Token not found. Please paste your token into '"+tokenFile+"'");
                    System.Environment.Exit(1);
                    return "";
                }

                return lines[0];
            }

            FileStream fileStream = new FileStream(tokenFile, FileMode.Create);
            //File.Create(tokenFile);
            byte[] info = new UTF8Encoding(true).GetBytes("REPLACE WITH YOUR TOKEN");
            fileStream.Write(info, 0, info.Length);
            Console.WriteLine("Config file: " + tokenFile + ", does not exist! It has been created for you. Please open it and correct the information");
            System.Environment.Exit(1);
            return "";
        }

        public async Task RegisterCommands()
        {
            client.MessageReceived += HandleCommandAsync;
            await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix("!", ref argPos)) {
                var result = await commandService.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess)
                {
                    Console.WriteLine(result.ErrorReason);
                }
            }

        }
    }
}
