using DSharpPlus.EventArgs;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TFABot
{
    public class clsEmail
    {
        static string EmailFromAddress;
        static string SMTPHost;
        static int SMTPPort;
        static string SMTPUsername;
        static string SMTPPassword;

        public clsEmail()
        {
        }

        static public void GetSettings()
        {
            if (Program.SettingsList.TryGetValue("Email-SMTPHost", out SMTPHost))
            {
                Program.SettingsList.TryGetValue("Email-Username", out SMTPUsername);
                Program.SettingsList.TryGetValue("Email-Password", out SMTPPassword);
                Program.SettingsList.TryGetValue("Email-FromAddress", out EmailFromAddress);

                if (Program.SettingsList.TryGetValue("Email-SMTPPort", out string port))
                {
                    int.TryParse(port, out SMTPPort);
                }
            }
        }

        static Task SendEmail(string To, string Subject, string Message, DSharpPlus.Entities.DiscordChannel ChBotAlert = null)
        {
            Task task = null;

            if (string.IsNullOrEmpty(SMTPHost))
            {
                if (ChBotAlert != null) ChBotAlert.SendMessageAsync($"No SMTP host set up");
                return task;
            }

            try
            {
                task = Task.Run(() =>
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(EmailFromAddress));
                    message.To.Add(new MailboxAddress(To));
                    message.Subject = Subject;

                    message.Body = new TextPart("plain") { Text = Message };

                    using (var client = new SmtpClient())
                    {
                        // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                        client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                        client.Connect(SMTPHost, SMTPPort, SecureSocketOptions.Auto);

                        // Note: only needed if the SMTP server requires authentication
                        client.Authenticate(SMTPUsername, SMTPPassword);
                        client.Timeout = 10000;
                        client.Send(message);
                        client.Disconnect(true);

                        if (ChBotAlert != null)
                            ChBotAlert.SendMessageAsync($"Sent e-mail {To}");
                    }
                });
            }
            catch (Exception ex)
            {
                if (ChBotAlert != null)
                    ChBotAlert.SendMessageAsync($"Send e-mail error {ex.Message}");
            }
            return task;
        }


        static public void EmailAlertList(string message = "")
        {
            if (!string.IsNullOrEmpty(SMTPHost))
            {
                string alarmMessage = $"{Program.BotName} Alarm {message}";
                foreach (var user in Program.UserList.Values.Where(x => x.OnDuty && !string.IsNullOrEmpty(x.email)))
                {
                    SendEmail(user.email, alarmMessage, alarmMessage);
                }
            }
        }

        static public void email(string names, DSharpPlus.Entities.DiscordChannel ChBotAlert = null)
        {
            if (string.IsNullOrEmpty(SMTPHost))
            {
                if (ChBotAlert != null) ChBotAlert.SendMessageAsync($"No SMTP host set up");
                return;
            }

            string alarmMessage = $"{Program.BotName} Alarm (manual Discord request)";

            foreach (var nameItem in names.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = nameItem.ToLower();
                if (!name.EndsWith("mail"))
                {
                    clsUser user;
                    if (!Program.UserList.TryGetValue(name, out user))
                    {
                        user = Program.UserList.Values.FirstOrDefault(x => x.DiscordName.ToLower() == name || x.Name.ToLower() == name);
                    }

                    if (user != null)
                        SendEmail(user.email, alarmMessage, alarmMessage, ChBotAlert);
                    else if (ChBotAlert != null)
                        ChBotAlert.SendMessageAsync("name not found!");
                }
            }

        }

        static public void email(MessageCreateEventArgs e)
        {
            var toRing = e.Message.Content.ToLower();
            email(toRing, e.Channel);
        }
    }
}


