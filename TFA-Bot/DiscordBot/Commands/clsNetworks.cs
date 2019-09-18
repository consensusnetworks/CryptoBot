using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsNetworks : IBotCommand
    {
        public string[] MatchCommand { get; private set; }
        public string[] MatchSubstring { get; private set; }
        public Regex[] MatchRegex { get; private set; }

        public clsNetworks()
        {
            MatchCommand = new[] { "networks", "network" };
        }

        public void Run(MessageCreateEventArgs e)
        {
            var cd = new ColumnDisplay();

            cd.AppendCol("Network");
            cd.AppendCol("Height");
            cd.AppendCol("Nodes");
            cd.AppendCol("Last block (utc)");
            cd.AppendCol("Next (est)");
            cd.AppendCol("Previous block times");

            cd.AppendCharLine('-');

            foreach (var network in Program.NetworkList.Values)
            {
                network.AppendDisplayColumns(ref cd);
                cd.NewLine();
            }
            e.Channel.SendMessageAsync($"```{cd.ToString()}```");
        }

        public void HelpString(ref ColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("networks", "", "Lists networks and their average blocktime");
        }
    }
}
