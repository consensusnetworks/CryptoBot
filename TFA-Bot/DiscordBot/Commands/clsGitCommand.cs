using DSharpPlus.EventArgs;
using System;
using System.Text;
using System.Text.RegularExpressions;
using TFABot.Git;

namespace TFABot.DiscordBot.Commands
{
    public class clsGitCommand : IBotCommand
    {

        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsGitCommand()
        {
            MatchCommand = new[] { "git" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            var msgSplit = e.Message.Content.Split(new char[] { ' ' });

            var message = new StringBuilder();
            message.Append("```");
            try
            {
                using (var git = new clsGit())
                {
                    if (msgSplit.Length > 1) git.Switch(msgSplit[1]);
                    message.AppendLine(git.ToString());
                }
            }
            catch (Exception ex)
            {
                message.AppendLine($"Error: {ex.Message}");
            }

            message.Append("```");


            if (msgSplit.Length > 1) message.AppendLine("`bot update` required to pull branch.");

            e.Channel.SendMessageAsync(message.ToString());
        }

        public void HelpString(ref ColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("git", "", "List branche(s)");
            columnDisplay.AppendCol("git", "<branch/commit>", "Checkout");
        }
    }
}
