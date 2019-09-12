using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsGoodAfternoon : IBotCommand
    {
        public string[] MatchCommand {get; private set;}
        public string[] MatchSubstring {get; private set;}
        public Regex[] MatchRegex {get; private set;}
        
        public clsGoodAfternoon()
        {
            MatchSubstring = new []{"afternoon"};
        }
        
        public void Run(MessageCreateEventArgs e)
        {
            e.Channel.SendMessageAsync("Good Afternoon! :sun_with_face:");
        }
        
        public void HelpString (ref clsColumnDisplay columnDisplay)
        {
            return;
        }
    }
}
