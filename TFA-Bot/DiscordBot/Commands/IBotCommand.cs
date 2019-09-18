using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public interface IBotCommand
    {
        string[] MatchCommand { get; }
        string[] MatchSubstring { get; }
        Regex[] MatchRegex { get; }

        void Run(MessageCreateEventArgs e);
        void HelpString(ref ColumnDisplay columnDisplay);
    }
}
