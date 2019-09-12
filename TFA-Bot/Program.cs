using DiscordBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TFABot
{
    public class Program
    {
        static enumRunState RunState = enumRunState.Run;
        public enum enumRunState : int
        {
            Stop = 0,
            Run = 1,
            Restart = 2,
            Update = 3,
            PreviousVersion = 4,
            MonoArgs = 5,
            Error = 100
        }

        static uint AlarmOffWarningMinutes = 30;

        static public Dictionary<string, string> SettingsList = new Dictionary<string, string>();
        static public Dictionary<string, clsUser> UserList = new Dictionary<string, clsUser>();
        static public Dictionary<string, clsNetwork> NetworkList = new Dictionary<string, clsNetwork>();
        static public Dictionary<string, clsNode> NodesList = new Dictionary<string, clsNode>();
        static public Dictionary<string, clsNodeGroup> NodeGroupList = new Dictionary<string, clsNodeGroup>();
        static public Dictionary<string, clsNotificationPolicy> NotificationPolicyList = new Dictionary<string, clsNotificationPolicy>();

        static public DateTime AppStarted = DateTime.UtcNow;
        static public clsAlarmManager AlarmManager = new clsAlarmManager();

        static public ManualResetEvent ApplicationHold = new ManualResetEvent(false);

        static public clsSpreadsheet Spreadsheet;
        static public clsBotClient Bot;

        static public string BotURL { get; private set; }
        static public string BotName;

        public enum EnumAlarmState { Off, On, Silent }

        static public DateTime AlarmOffTime;
        static public uint AlarmOffWarningMultiplier;

        static public DateTime? AlarmStateTimeout;
        static public EnumAlarmState _alarmState = EnumAlarmState.On;
        static public EnumAlarmState AlarmState
        {
            get
            {
                return _alarmState;
            }

            private set
            {
                if (_alarmState != value)
                {
                    _alarmState = value;
                    if (value == EnumAlarmState.Off)
                    {
                        AlarmOffTime = DateTime.UtcNow;
                        AlarmOffWarningMultiplier = 0;
                    }
                    else if (value == EnumAlarmState.On)
                    {
                        AlarmStateTimeout = null;
                    }
                }
            }
        }

        public static int Main(string[] args)
        {
            AlarmState = EnumAlarmState.On;

            try
            {
                Console.WriteLine($"Git commit: {clsVersion.GitCommitHash}");
                if (clsVersion.VersionChangeFlag) Console.WriteLine("NEW VERSION DETECTED");

                Console.WriteLine($"ARGS={Environment.CommandLine}");
                BotURL = Environment.GetEnvironmentVariable("BOTURL");

                if (string.IsNullOrEmpty(BotURL))
                {
                    Console.WriteLine("'BOTURL' Google Spreadsheet URL missing.");
                    return (int)enumRunState.Error;
                }
                Console.WriteLine($"URL={BotURL}");

                Spreadsheet = new clsSpreadsheet(BotURL);
                var log = Spreadsheet.LoadSettings();
                Console.WriteLine(log);

                SettingsList.TryGetValue("BotName", out BotName);

                string value;
                if (SettingsList.TryGetValue("AlarmOffWarningMinutes", out value)) uint.TryParse(value, out AlarmOffWarningMinutes);

                string DiscordToken;
                if (!SettingsList.TryGetValue("Discord-Token", out DiscordToken))
                {
                    Console.WriteLine("Discord-Token not found");
                    return (int)enumRunState.Error;
                }

                clsEmail.GetSettings();

                Console.WriteLine("Starting monitoring");
                const int apiTimeout = 2000;
                const int loopWait = 3000;

                DateTime LastBotStart = DateTime.UtcNow;
                int BotErrorCounter = 0;
                using (Bot = new clsBotClient(DiscordToken))
                {
                    Bot.RunAsync();
                    if (log.Contains("rror:")) Bot.SendAlert($"```{log}```");

                    while (RunState == enumRunState.Run)
                    {
                        //Query every node.
                        foreach (var node in NodesList.Values.Where(x => x.Monitor))
                        {
                            node.GetHeightAsync(apiTimeout);
                        }

                        ApplicationHold.WaitOne(apiTimeout); //Wait for the timeout

                        foreach (var group in NodeGroupList.Values)
                        {
                            group.Monitor();
                        }

                        foreach (var network in NetworkList.Values)
                        {
                            network.CheckStall();
                        }

                        if (AlarmStateTimeout.HasValue)
                        {
                            if (AlarmStateTimeout.Value < DateTime.UtcNow) AlarmState = EnumAlarmState.On;
                        }
                        else if (AlarmState == EnumAlarmState.Off && (DateTime.UtcNow - AlarmOffTime).TotalMinutes > (AlarmOffWarningMinutes * (AlarmOffWarningMultiplier + 1)))
                        {
                            Bot.Our_BotAlert.SendMessageAsync($"Warning, the Alarm has been off {(DateTime.UtcNow - AlarmOffTime).TotalMinutes:0} minutes.  Forget to reset it?");
                            AlarmOffWarningMultiplier++;
                        }

                        AlarmManager.Process();

                        //Check the status of the Discord connection.  If it disconnects, it doesn't always restart.
                        if (!Bot.LastHeartbeatd.HasValue || (DateTime.UtcNow - Bot.LastHeartbeatd.Value).TotalSeconds > 90)
                        {
                            if ((DateTime.UtcNow - LastBotStart).TotalSeconds > 120)
                            {
                                Console.WriteLine("Discord not connected. Restarting...");
                                BotErrorCounter++;
                                if (BotErrorCounter == 2) clsEmail.EmailAlertList("Bot: Discord not connected.");
                                Bot._client.Dispose();
                                LastBotStart = DateTime.UtcNow;
                                Bot = new clsBotClient(DiscordToken);
                                Bot.RunAsync();
                            }
                        }
                        else
                        {
                            BotErrorCounter = 0;
                        }

                        ApplicationHold.WaitOne(loopWait);
                    }

                    Console.WriteLine($"Exit Code: {RunState} ({(int)RunState})");

                    switch (RunState)
                    {
                        case enumRunState.Update:
                            Bot.Our_BotAlert.SendMessageAsync("Shutting down to update. Back soon. :grin:").Wait(1000);
                            break;
                        case enumRunState.Restart:
                            Bot.Our_BotAlert.SendMessageAsync("Shutting down to restart. :relieved:").Wait(1000);
                            break;
                        case enumRunState.MonoArgs:
                            Bot.Our_BotAlert.SendMessageAsync("Shutting down to restart in Debug mode. :spy:").Wait(1000);
                            break;
                        case enumRunState.Stop:
                            Bot.Our_BotAlert.SendMessageAsync("Goodbye! :sleeping:").Wait(1000);
                            break;
                    }
                }
                return (int)RunState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                return (int)enumRunState.Error;
            }
        }

        static public void SendAlert(string message)
        {
            if (AlarmState == EnumAlarmState.On || AlarmState == EnumAlarmState.Silent)
                Bot.Our_BotAlert.SendMessageAsync(message);
        }

        static public void SetRunState(enumRunState runState)
        {
            RunState = runState;
            ApplicationHold.Set();
        }

        static public string GetNodes()
        {
            var cd = new clsColumnDisplay();

            cd.AppendCol("Node");
            cd.AppendCol("Host");
            cd.AppendCol("Ver");
            cd.AppendCol("Height");
            cd.AppendCol("Ave reply");

            cd.AppendCharLine('-');

            foreach (var group in NodeGroupList)
            {
                foreach (var node in NodesList.Values.Where(x => x.Group == group.Key))
                {
                    node.AppendDisplayColumns(ref cd);
                    cd.NewLine();
                }
            }
            return cd.ToString();
        }

        static public void SetAlarmState(EnumAlarmState state, TimeSpan? timeout = null)
        {
            if (timeout.HasValue) AlarmStateTimeout = DateTime.UtcNow.Add(timeout.Value);
            AlarmState = state;
        }
    }
}
