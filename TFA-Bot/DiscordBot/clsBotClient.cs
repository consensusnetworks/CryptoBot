﻿using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TFABot;
using TFABot.DiscordBot;
using static TFABot.Program;

namespace DiscordBot
{
    public class BotClient : IDisposable
    {
        public const ulong FactomServerID = 419201548372017163;
        public const ulong FactomOperatorAlertChannel = 443025488655417364;
        public const string FactomOperatorAlertChannelString = "operators-alerts";

        clsAlarm AlarmNoFactomChannel;

        public DiscordClient _client;
        private CancellationTokenSource _cts;
        private clsCommands Commands = new clsCommands();

        public DateTime TimeConnected { get; private set; }
        public DateTime? LastHeartbeatd { get; private set; }


        public DSharpPlus.Entities.DiscordChannel Our_BotAlert = null;
        public DSharpPlus.Entities.DiscordChannel Factom_BotAlert = null;

        StringBuilder TextBuffer = new StringBuilder();

        public static BotClient Instance = null;

        public BotClient(string Token)
        {
            Instance = this;

            _client = new DiscordClient(new DiscordConfiguration()
            {
                AutoReconnect = true,
                EnableCompression = true,
                LogLevel = LogLevel.Error,
                Token = Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });
            _client.SetWebSocketClient<WebSocketSharpClient>();
            _client.Ready += OnReadyAsync;
            _client.GuildAvailable += this.Client_GuildAvailable;
            _client.ClientErrored += this.Client_ClientError;
            _client.MessageCreated += MessageCreateEvent;
            _client.Heartbeated += Heartbeated;

            Commands.LoadCommandClasses();

            AlarmNoFactomChannel = new clsAlarm(clsAlarm.enumAlarmType.Error, "Factom #operators-alert channel not connected. Please make sure you have had permission set.", new TimeSpan(0, 1, 0));
            AlarmManager.New(AlarmNoFactomChannel);
        }
        //Incoming Discord Message
        private async Task MessageCreateEvent(MessageCreateEventArgs e)
        {
            try
            {
                if (e.Author.IsBot) return;  //Reject our own messages

                if (e.Channel == Factom_BotAlert)
                {

                    Our_BotAlert.SendMessageAsync($"#operators-alert:```{e.Message.Content}```");
                    ProcessFactomAlarm(e.Message.Content);
                    return;
                }

                if (e.Channel.GuildId == FactomServerID) return;  //Ignore Factom's Discord server

                Commands.DiscordMessage(e); //Forward message to commands lookup.

                if (Our_BotAlert == null)
                {
                    e.Channel.SendMessageAsync("WARNING: Your bot alert channel is not found. Check the settings tab for the correct name, and discord permissions");
                }

                Console.Write(e.Message);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        public async Task RunAsync()
        {
            await _client.ConnectAsync();
            await WaitForCancellationAsync();
        }

        private async Task WaitForCancellationAsync()
        {
            while (!_cts.IsCancellationRequested)
                await Task.Delay(500);
        }

        private async Task OnReadyAsync(ReadyEventArgs e)
        {
            await Task.Yield();
            TimeConnected = DateTime.Now;
        }

        private async Task Heartbeated(HeartbeatEventArgs eventArgs)
        {
            LastHeartbeatd = DateTime.UtcNow;
        }


        //Discord Server connected
        private Task Client_GuildAvailable(GuildCreateEventArgs e)
        {
            Console.WriteLine($"Discord Server: {e.Guild.Name} {e.Guild.Id}");

            if (e.Guild.Id == FactomServerID)  //Factom's Discord server
            {
                Factom_BotAlert = e.Guild.Channels.FirstOrDefault(x => x.Id == FactomOperatorAlertChannel);
                if (Factom_BotAlert == null)
                {
                    Console.WriteLine("Warning: Factom ID not found");
                    //We will try finding the name instead.
                    Factom_BotAlert = e.Guild.Channels.FirstOrDefault(x => x.Name.Contains(FactomOperatorAlertChannelString));
                }
                if (Factom_BotAlert == null)
                {
                    SendAlert("Warning: Factom operators-alarts not found");
                }
                else
                {
                    Console.WriteLine($"Factom Alert channel: {Factom_BotAlert.Name}");

                    var me = e.Guild.Members.FirstOrDefault(x => x.Id == _client.CurrentUser.Id);
                    if (me != null)
                    {
                        var permissions = Factom_BotAlert.PermissionsFor(me);
                        if (permissions.HasPermission(Permissions.AccessChannels))
                        {
                            AlarmManager.Clear(AlarmNoFactomChannel);
                        }
                    }
                    else
                    {
                        SendAlert("Warning: My user not found in Discord");
                    }
                }
            }
            else
            {
                string alertChannelString;
                if (SettingsList.TryGetValue("Discord-AlertsChannel", out alertChannelString))
                {
                    alertChannelString = alertChannelString.ToLower().Replace("#", "");
#if DEBUG
                    Our_BotAlert = e.Guild.Channels.FirstOrDefault(x => x.Name.Contains("bot-in-debug"));
                    if (Our_BotAlert == null) Our_BotAlert = e.Guild.Channels.FirstOrDefault(x => x.Name == alertChannelString);
#else
                    Our_BotAlert = e.Guild.Channels.FirstOrDefault(x=>x.Name == alertChannelString);
                    if (Our_BotAlert!=null)
                    {
                       Console.WriteLine($"Our Alert channel: {Our_BotAlert.Name}");
                       if (clsVersion.VersionChangeFlag)
                         Bot.Our_BotAlert.SendMessageAsync($":drum: Welcome! (removing version number because it's causing it not to compile) :trumpet:");
                       else
                         Bot.Our_BotAlert.SendMessageAsync("Hello :innocent:");
                         
                       if (TextBuffer.Length>0) Our_BotAlert.SendMessageAsync(TextBuffer.ToString()).ContinueWith((x)=>{TextBuffer.Clear();});
                    }
                    else
                    {
                        Console.WriteLine($"ERROR: Can't find AlertChannel {alertChannelString}");
                    }
#endif                                    
                }
                else
                {
                    SendAlert("Warning: Factom operators-alarts not found");
                    Console.WriteLine("Warning: Discord-AlertsChannel not set");
                }
            }

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        public void SendAlert(string text)
        {
            Console.WriteLine(text);
            if (Our_BotAlert != null)
                Our_BotAlert.SendFileAsync(text);
            else
                TextBuffer.AppendLine(text);
        }

        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            Console.WriteLine($"Bot Exception: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        void ProcessFactomAlarm(string message)
        {
            bool emergency = message.Contains("EMERGENCY");
            var messageSplit = message.Split(' ');

            List<string> keywordUsed = new List<string>();
            List<string> toCall = new List<string>();

            //Loop through all OnDuty users
            foreach (var user in Program.UserList.Values.Where(x => x.KeywordAlert != null && x.OnDuty))
            {
                foreach (var word in messageSplit)
                {
                    foreach (var keyword in user.KeywordAlert)
                    {
                        if (emergency && keyword == "E" || (keyword != "E" && word == keyword))
                        {
                            if (!keywordUsed.Contains(keyword)) keywordUsed.Add(keyword);
                            if (!toCall.Contains(user.Name)) toCall.Add(user.Name);
                        }
                    }
                }
            }

            //Loop through OffDuty users, triggering keywords that have not been picked up by OnDuty users
            foreach (var user in Program.UserList.Values.Where(x => x.KeywordAlert != null && !x.OnDuty))
            {
                foreach (var word in messageSplit)
                {
                    foreach (var keyword in user.KeywordAlert.Where(x => !keywordUsed.Contains(x)))
                    {
                        if (emergency && keyword == "E" || (keyword != "E" && word == keyword))
                        {
                            if (!keywordUsed.Contains(keyword)) keywordUsed.Add(keyword);
                            if (!toCall.Contains(user.Name)) toCall.Add(user.Name);
                        }
                    }
                }
            }

            foreach (var userName in toCall)
            {
                clsDialler.call(userName);
                clsEmail.email(userName);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
