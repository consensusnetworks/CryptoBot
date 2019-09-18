using DSharpPlus.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TFABot.DiscordBot.Commands;

namespace TFABot.DiscordBot
{
    public class clsCommands
    {
        List<(string, IBotCommand)> MatchCommand = new List<(string, IBotCommand)>();
        List<(string, IBotCommand)> MatchSubstring = new List<(string, IBotCommand)>();
        List<(Regex, IBotCommand)> MatchRegex = new List<(Regex, IBotCommand)>();

        public static clsCommands Instance;
        public static string BotCommandPrefix { get; set; }


        public clsCommands()
        {
            Instance = this;
            string prefix;
            if (Program.SettingsList.TryGetValue("BotCommandPrefix", out prefix))
            {
                BotCommandPrefix = prefix;
            }
        }


        public void DiscordMessage(MessageCreateEventArgs e)
        {

            string Message;
            try
            {
                if (string.IsNullOrEmpty(e.Message.Content)) return;

                if (string.IsNullOrEmpty(BotCommandPrefix))
                {
                    Message = e.Message.Content;
                }
                else if (e.Message.Content.StartsWith(BotCommandPrefix) &&
                        (e.Message.Content.Length > BotCommandPrefix.Length))
                {
                    Message = e.Message.Content.Substring(BotCommandPrefix.Length);
                }
                else
                {
                    return;
                }

                var lowMessage = Message.ToLower();
                var firstword = lowMessage.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                foreach (var command in MatchCommand.Where(x => firstword[0] == x.Item1))
                {
                    command.Item2.Run(e);
                }

                foreach (var command in MatchSubstring.Where(x => lowMessage.Contains(x.Item1)))
                {
                    command.Item2.Run(e);
                }

                foreach (var command in MatchRegex.Where(x => x.Item1.IsMatch(Message)))
                {
                    command.Item2.Run(e);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DiscordMessage Error: {ex.Message}");
            }
        }

        public void LoadCommandClasses()
        {
            var type = typeof(IBotCommand);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

            foreach (var commandType in types)
            {
                IBotCommand command = (IBotCommand)Activator.CreateInstance(commandType);

                if (command.MatchCommand != null)
                    foreach (var match in command.MatchCommand) { MatchCommand.Add((match, command)); }
                if (command.MatchSubstring != null)
                    foreach (var match in command.MatchSubstring) { MatchSubstring.Add((match, command)); }
                if (command.MatchRegex != null)
                    foreach (var match in command.MatchRegex) { MatchRegex.Add((match, command)); }
            }
        }

        public string GetHelpString(IBotCommand command = null)
        {
            try
            {
                var columnDisplay = new ColumnDisplay
                {
                    ColumnChar = ' '
                };

                columnDisplay.AppendLine($"ConsensusBot                                      Uptime {(DateTime.UtcNow - Program.AppStarted).ToDHMDisplay() }");
                columnDisplay.AppendCol("Command");
                columnDisplay.AppendCol("Args");
                columnDisplay.AppendCol("Description");

                columnDisplay.AppendCharLine('-');

                if (command != null)
                {
                    command.HelpString(ref columnDisplay);
                }
                else
                {
                    foreach (var commandItem in MatchCommand.DistinctBy(x => x.Item2).OrderBy(x => x.Item1))
                    {
                        commandItem.Item2.HelpString(ref columnDisplay);
                        columnDisplay.NewLine();
                    }
                }
                //cd.Append(Program.BotURL);
                return columnDisplay.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }
    }
}
