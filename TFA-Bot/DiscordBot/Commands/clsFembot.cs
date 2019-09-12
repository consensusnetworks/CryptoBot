using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsFembot : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsFembot()
        {
            MatchSubstring = new[] { "fembot" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            e.Channel.SendMessageAsync("*ANGRY BOT :rage: ");
        }

        public void HelpString(ref clsColumnDisplay columnDisplay)
        {
            return;
        }
    }
}
