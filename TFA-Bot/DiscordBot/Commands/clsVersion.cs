using DSharpPlus.EventArgs;
using System.Text;
using System.Text.RegularExpressions;
using TFABot.Git;

namespace TFABot.DiscordBot.Commands
{
    public class clsVersion : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsVersion()
        {
            MatchCommand = new[] { "version" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            var message = new StringBuilder();
            message.Append("```");
            message.AppendLine(clsGitHead.GetHeadToString());
            message.Append("```");
            e.Channel.SendMessageAsync(message.ToString());
        }

        public void HelpString(ref ColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("version");
        }
    }
}
