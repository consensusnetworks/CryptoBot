using DSharpPlus.EventArgs;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsDiscord : IBotCommand
    {

        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsDiscord()
        {
            MatchCommand = new[] { "discord" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            var sb = new StringBuilder();

            if (Program.SettingsList.TryGetValue("Discord-AlertsChannel", out string alertChannelString))
                alertChannelString = alertChannelString.ToLower().Replace("#", "");

            sb.AppendLine($"Discord Gateway: {Program.Bot._client.GatewayUrl} Version: {Program.Bot._client.GatewayVersion}");
            sb.AppendLine($"                 {Program.Bot._client.CurrentUser}   ");
            sb.AppendLine($"                 Ping {Program.Bot._client.Ping}");

            sb.AppendLine();
            foreach (var discord in Program.Bot._client.Guilds.Values)
            {
                sb.AppendLine($"{discord.Name} {discord.Id} {discord.RegionId}");
                sb.AppendLine($"   Channels:{discord.Channels.Count} Members:{discord.MemberCount} Owner:{discord.Owner.DisplayName}#{discord.Owner.Discriminator}");
                sb.AppendLine($"   Joined:{discord.JoinedAt:yyyy-MM-dd}");

                if (discord.Id != 419201548372017163)
                {
                    sb.Append($"   Alert Channel: '{alertChannelString}' ");
                    sb.AppendLine(discord.Channels.Any(x => x.Name == alertChannelString) ? "" : "NOT FOUND");
                }
                sb.AppendLine();
            }


            e.Channel.SendMessageAsync($"```{sb.ToString()}```");
        }

        public void HelpString(ref clsColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("discord", "", "Display discord status");
        }
    }
}
