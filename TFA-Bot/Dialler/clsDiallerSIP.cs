using DiscordBot;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TFABot.Dialler
{
    public class clsDiallerSIP : IDialler
    {

        string Username;
        string Password;
        string Host;
        string CallingNumber;

        public clsDiallerSIP()
        {
            Username = Program.SettingsList["SIP-Username"];
            Password = Environment.GetEnvironmentVariable("SIP-PASSWORD") ?? Program.SettingsList["SIP-Password"];
            Host = Program.SettingsList["SIP-Host"];
            CallingNumber = Program.SettingsList["SIP-CallingNumber"];
        }


        public Task CallAsync(string Name, string Number, DSharpPlus.Entities.DiscordChannel ChBotAlert = null)
        {
            Task task;
            try
            {
                if (ChBotAlert == null) ChBotAlert = BotClient.Instance.Our_BotAlert;

                task = Task.Run(() =>
                {
                    string sipp = "/app/sipp/sipp";
                    string timeout = "30s";
                    string dialplanPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data/dialplan.xml");

                    string perms = $"{Host} -au {Username} -ap {Password} -l 1 -m 1 -sf {dialplanPath} -timeout {timeout} -s {Number.Replace(" ", "")}";

                    var process = new Process
                    {
                        StartInfo =
                        {
                            FileName = sipp,
                            Arguments = perms,
                            UseShellExecute = false
                            //RedirectStandardOutput = true,
                            //RedirectStandardError = true
                        },
                    };

                    ChBotAlert.SendMessageAsync($"Calling {Name} {Number}");
                    process.Start();
                    if (!process.WaitForExit(60000))
                    {
                        ChBotAlert.SendMessageAsync($"{Name} Call timed out.");
                    }
                    else
                    {
                        ChBotAlert.SendMessageAsync($"{Name} Call Ended.");
                        process.Dispose();
                    }
                });
                return task;
            }
            catch (Exception ex)
            {
                if (ChBotAlert != null) ChBotAlert.SendMessageAsync($"Call error: {ex.Message}");
                Console.Write("Call error: " + ex.Message);
            }
            return null;
        }
    }
}
