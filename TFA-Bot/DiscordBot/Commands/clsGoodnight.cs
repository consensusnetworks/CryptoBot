using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsGoodnight : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsGoodnight()
        {
            MatchCommand = new[] { "night" };
            MatchSubstring = new[] { "Night!", "goodnight", "good night" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            e.Channel.SendMessageAsync("Goodnight! :sleeping:");
        }

        public void HelpString(ref clsColumnDisplay columnDisplay)
        {
            return;
        }
    }
}
