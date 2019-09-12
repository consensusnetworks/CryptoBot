﻿using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsSkynet : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsSkynet()
        {
            MatchCommand = new[] { "skynet" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            e.Channel.SendMessageAsync("Skynet Activated");
        }

        public void HelpString(ref clsColumnDisplay columnDisplay)
        {
            return;
        }
    }
}
