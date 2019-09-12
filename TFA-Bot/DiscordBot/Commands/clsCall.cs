using DSharpPlus.EventArgs;
using System.Text.RegularExpressions;

namespace TFABot.DiscordBot.Commands
{
    public class clsCall : IBotCommand
    {
        public string[] MatchCommand {get; private set;}
        public string[] MatchSubstring {get; private set;}
        public Regex[] MatchRegex {get; private set;}
        
        public clsCall()
        {
            MatchCommand = new []{"call"};
        }
        
        public void Run(MessageCreateEventArgs e)
        {
            clsDialler.call(e);
        }
        
        public void HelpString (ref clsColumnDisplay columnDisplay)
        {
            columnDisplay.AppendCol("call","<user>");
        }
    }
}
