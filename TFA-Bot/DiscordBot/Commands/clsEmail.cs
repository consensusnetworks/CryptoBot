using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsEmailCommand : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsEmailCommand()
        {
            MatchCommand = new[] { "email" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            clsEmail.email(e);
        }

        public void HelpString(ref ColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("email", "<user>");
        }
    }
}
