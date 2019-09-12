using System.Threading.Tasks;

namespace TFABot.Dialler
{
    public interface IDialler
    {
        Task CallAsync(string Name, string Number, DSharpPlus.Entities.DiscordChannel ChBotAlert = null);
    }
}
