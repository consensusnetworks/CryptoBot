using DSharpPlus.EventArgs;
using System;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsHelp : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsHelp()
        {
            MatchCommand = new[] { "help" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            try
            {
                var helpText = clsCommands.Instance.GetHelpString();

                foreach (var text in helpText.SplitAfter(1500))
                {
                    e.Channel.SendMessageAsync($"```{text}```");
                }
                e.Channel.SendMessageAsync($"Settings: {Program.BotURL}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void HelpString(ref clsColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("help");
        }
    }
}
