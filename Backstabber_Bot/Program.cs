using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace Backstabber_Bot
{
    class Program
    {
        private DiscordSocketClient Client;
        private CommandService Commands;

        static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();


        private async Task MainAsync()
        {
            Client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });

            Commands = new CommandService(new CommandServiceConfig
            {
                CaseSensitiveCommands = false,
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Info
            });


            Client.MessageReceived += Client_MessageReceived;
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), services: null);

            Client.Ready += Client_Ready;
            Client.Log += Client_Log;


            await Client.LoginAsync(TokenType.Bot, "NTM4ODU5OTczODA0MDMyMDIw.Dy5-Mw.N5HoMBpuda59_MVhL46pn3Ph9Ro");
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage Message)
        {
            Console.WriteLine($"{DateTime.Now} at {Message.Source} {Message.Message}");
        }

        private async Task Client_Ready()
        {
            await Client.SetGameAsync("Scheming...");
        }

        private async Task Client_MessageReceived(SocketMessage MessageParam)
        { 
            var Message = MessageParam as SocketUserMessage;
            var Context = new SocketCommandContext(Client, Message);

            if (Context.Message == null || Context.Message.Content == "") return;
            if (Context.User.IsBot) return;

            int argPos = 0;
            if (!(Message.HasStringPrefix("~", ref argPos) || Message.HasMentionPrefix(Client.CurrentUser, ref argPos))) return;

            var result = await Commands.ExecuteAsync(Context, argPos, services: null);
            if (!result.IsSuccess)
                await Context.Channel.SendMessageAsync(result.ErrorReason);

        }
    }
}

namespace Backstabber_Bot.Core.Commands
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        public static Dictionary<ulong, int> idDecoder = new Dictionary<ulong, int>()
        {
            {84134882262945792, 0},
            {118540002056470530, 1},
            {84025918531567616, 2},
            {84021217073336320, 3},
            {84031205044912128, 4},
            {230855803756740612, 5},
            {113453835829149696, 6}
        };
        public static ulong[] userIds = new ulong[7] 
        { 84134882262945792 ,
          118540002056470530 ,
          84025918531567616,
          84021217073336320,
          84031205044912128,
          230855803756740612,
          113453835829149696};
        public static string[] countries = new string[7]
        {
            "Austria Hungary",
            "Britain",
            "France",
            "Germany",
            "Italy",
            "Russia",
            "Turkey"
        };

        public static bool[] moveInput = new bool[7]
        {
            Properties.Settings.Default.AH,
            Properties.Settings.Default.BR,
            Properties.Settings.Default.FR,
            Properties.Settings.Default.GER,
            Properties.Settings.Default.IT,
            Properties.Settings.Default.RUS,
            Properties.Settings.Default.TUR

        };

        [Command("Help"), Summary("Help printout.")]
        public async Task Help()
        {
            await Context.Channel.SendMessageAsync("```\nI am the diplomacy discord bot \n\n" +
                                                   "your discord id is already associated to your respective countries\n\n" +
                                                   "to enter your moves type ~moves \"\" into my dm with you. Please send in a single message(if you dont it wont work).\nfor more info type ~example.\n" +
                                                   "\n\nother commands:\n" +
                                                   "~whoami - to check if your country is correct. (message scott if it isnt)\n```");

        }

        [Command("example"), Summary("prints your example moves")]
        public async Task example()
        {
            await Context.Channel.SendMessageAsync("Example move input:\n" +
                                                   "```\n~moves\n" +
                                                   "\"F ABC - DEF\nA GHI - KLM\"\n```");

        }

        [Command("moves"), Alias("move"), Summary("Diplomacy moves input.")]
        public async Task Moves(string moves)
        {
            
            if (Context.IsPrivate)
            {
                string country = countries[idDecoder[Context.User.Id]];
                moveInput[idDecoder[Context.User.Id]] = true;
                await Context.Channel.SendMessageAsync($"Entering moves for {country}.");
                File.WriteAllText($@"C:\Users\frenk\source\repos\Backstabber_Bot\Backstabber_Bot\moves\{country}.txt", moves);
                switch (idDecoder[Context.User.Id])
                {
                    case 0:
                        Properties.Settings.Default.AH = moveInput[0];
                        break;
                    case 1:
                        Properties.Settings.Default.BR = moveInput[1];
                        break;
                    case 2:
                        Properties.Settings.Default.FR = moveInput[2];
                        break;
                    case 3:
                        Properties.Settings.Default.GER = moveInput[3];
                        break;
                    case 4:
                        Properties.Settings.Default.IT = moveInput[4];
                        break;
                    case 5:
                        Properties.Settings.Default.RUS = moveInput[5];
                        break;
                    case 6:
                        Properties.Settings.Default.TUR = moveInput[6];
                        break;
                    default:
                        break;
                }
                Properties.Settings.Default.Save();
            }
            else
            {
                await Context.Channel.SendMessageAsync("This is not a private channel!");
            }
        }

        [Command("whoami"), Summary("Check to see you are the correct country")]
        public async Task whoAmI()
        {
            await Context.Channel.SendMessageAsync($"You are {countries[idDecoder[Context.User.Id]]}.");
        }

        [Command("movecheck"), Alias("movec","mc"), Summary("Checks who hasnt input moves yet")]
        public async Task Check()
        {
            if(Context.User.Id == 118540002056470530 || Context.User.Id == 84021217073336320)
            {
                string output = "These countries have not input moves: ";
                for (int i = 0; i < 7; i++)
                {
                    if (!moveInput[i])
                    {
                        output += countries[i] + " " + $"<@{userIds[i]}>" + (i == 6 ? "." : ", ");
                    }
                }
                if(output == "These countries have not input moves: ")
                {
                    await Context.Channel.SendMessageAsync("Everyone has input their moves!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync(output);
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("You do not have permission to use this command.");
                return;
            }
            

        }

        [Command("sendMoves"), Alias("printmoves","sm"), Summary("Print moves")]
        public async Task printMoves(string force = "")
        {
            if (Context.User.Id == 118540002056470530 || Context.User.Id == 84021217073336320)
            {
                foreach (bool move in moveInput)
                {
                    if (!move && force != "-f")
                    {
                        string error = "These countries have not input moves: ";
                        for (int i = 0; i < 7; i++)
                        {
                            if (!moveInput[i])
                            {
                                error += countries[i] + " " + $"<@{userIds[i]}>" + (i == 6 ? "." : ", ");
                            }
                        }
                        await Context.Channel.SendMessageAsync(error);
                        return;
                    }
                }
                for (int i = 0; i < 7; i++)
                {
                    moveInput[i] = false;
                }
                Properties.Settings.Default.AH = false;
                Properties.Settings.Default.BR = false;
                Properties.Settings.Default.FR = false;
                Properties.Settings.Default.GER = false;
                Properties.Settings.Default.IT = false;
                Properties.Settings.Default.RUS = false;
                Properties.Settings.Default.TUR = false;
                string output = (Properties.Settings.Default.season ? "```\nfall" : "```\nspring") + " " + Properties.Settings.Default.year;
                if (Properties.Settings.Default.season)
                {
                    Properties.Settings.Default.year++;
                }
                Properties.Settings.Default.season = !Properties.Settings.Default.season;
                for (int i = 0; i < 7; i++)
                {
                    try
                    {
                        output += "\n" + countries[i] + ":\n" + File.ReadAllText($@"C:\Users\frenk\source\repos\Backstabber_Bot\Backstabber_Bot\moves\{countries[i]}.txt");
                        File.WriteAllText($@"C:\Users\frenk\source\repos\Backstabber_Bot\Backstabber_Bot\moves\{countries[i]}.txt", "");
                    }catch (Exception e){
                        continue;
                    }
                }
                await Context.Channel.SendMessageAsync(output + "\n```");
                Properties.Settings.Default.Save();
            }
            else
            {
                await Context.Channel.SendMessageAsync("You do not have permission to use this command.");
                return;
            }
        }

        [Command("resetAll"), Summary("resets all the game variables")]
        public async Task ResetAll()
        {
            Properties.Settings.Default.AH = false;
            Properties.Settings.Default.BR = false;
            Properties.Settings.Default.FR = false;
            Properties.Settings.Default.GER = false;
            Properties.Settings.Default.IT = false;
            Properties.Settings.Default.RUS = false;
            Properties.Settings.Default.TUR = false;
            Properties.Settings.Default.year = 1901;
            Properties.Settings.Default.season = false;
            Properties.Settings.Default.Save();
            for (int i = 0; i < 7; i++)
            {
                File.WriteAllText($@"C:\Users\frenk\source\repos\Backstabber_Bot\Backstabber_Bot\moves\{countries[i]}.txt", "");
            }
            await Context.Channel.SendMessageAsync("All game variables reset...");
        }
    }
}



//admin: 84021217073336320