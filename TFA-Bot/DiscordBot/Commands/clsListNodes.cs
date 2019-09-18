using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsListNodes : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsListNodes()
        {
            MatchCommand = new[] { "nodes" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            e.Channel.SendMessageAsync($"```{TFABot.Program.GetNodes()}```");
        }

        public void HelpString(ref ColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("nodes", "", "List nodes.");
        }
    }
}
