﻿using DSharpPlus.EventArgs;
using System.Text;
using System.Text.RegularExpressions;
using TFABot.DiscordBot.Commands;

namespace TFABot
{
    public class clsUsers : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsUsers()
        {
            MatchCommand = new[] { "users" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append("```");
            foreach (var user in Program.UserList.Values)
            {
                sb.Append($"{user.Name.PadRight(15)} {user.GetUserTime():yy-MM-dd HH:mm}");
                if (user.OnDuty) sb.Append($"   ON DUTY");
                sb.AppendLine();
            }
            sb.Append("```");
            e.Channel.SendMessageAsync(sb.ToString());
        }

        public void HelpString(ref ColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("users", "", "List users.");
        }
    }
}
