﻿using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Threading.Tasks;

namespace TFABot.Dialler
{
    public class clsDiallerTwilio : IDialler
    {
        string Username;
        string Password;
        string Host;
        string CallingNumber;
        string TwimletURL;

        public clsDiallerTwilio()
        {
            Username = Program.SettingsList["SIP-Username"];
            Password = Environment.GetEnvironmentVariable("SIP-PASSWORD") ?? Program.SettingsList["SIP-Password"];
            Host = Program.SettingsList["SIP-Host"];
            CallingNumber = Program.SettingsList["SIP-CallingNumber"];

            Program.SettingsList.TryGetValue("TwimletURL", out TwimletURL);
            if (string.IsNullOrEmpty(TwimletURL)) TwimletURL = "http://demo.twilio.com/docs/voice.xml";

        }

        public Task CallAsync(string Name, string Number, DSharpPlus.Entities.DiscordChannel ChBotAlert = null)
        {
            try
            {
                return Task.Run(() =>
                {

                    Number = string.Join("", Number.Split('(', ')', '-', ' '));

                    var client = new RestClient($"https://api.twilio.com/2010-04-01/Accounts/{Username}")
                    {
                        Timeout = 5000,
                        Authenticator = new HttpBasicAuthenticator(Username, Password)
                    };

                    var request = new RestRequest("Calls", Method.POST);
                    request.AddParameter("Url", TwimletURL);
                    request.AddParameter("To", $"{Number}");
                    request.AddParameter("From", $"{CallingNumber}");

                    IRestResponse response = client.Execute(request);

                    if (response.ResponseStatus == ResponseStatus.Completed && ChBotAlert != null)
                    {
                        var content = response.Content;
                        var pt1 = response.Content.IndexOf("<Message>");
                        if (pt1 > 0)
                        {
                            var pt2 = response.Content.IndexOf("</M", pt1);
                            ChBotAlert.SendMessageAsync($"{Name} {response.Content.Substring(pt1 + 9, pt2 - pt1 - 9)}");
                        }
                        else if (response.Content.Contains("AnsweredBy"))
                        {
                            ChBotAlert.SendMessageAsync($"{Name} call in progress");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                if (ChBotAlert != null)
                    ChBotAlert.SendMessageAsync($"Call error: {ex.Message}");

                Console.Write($"Call error: {ex.Message}");
                return null;
            }
        }
    }
}
