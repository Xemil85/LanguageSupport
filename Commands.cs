using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

public class Commands : ModuleBase<SocketCommandContext>
{
    private readonly GroqAPI _groqClient;

    public Commands(GroqAPI groqClient)
    {
        _groqClient = groqClient;
    }

    public async Task HandleSlashCommandAsync(SocketSlashCommand command)
    {
        switch (command.CommandName)
        {
            case "simplify":
                await HandleSimplifyCommand(command);
                break;

            default:
                await command.RespondAsync("Tuntematon komento.");
                break;
        }
    }

    private async Task HandleSimplifyCommand(SocketSlashCommand command)
    {
        var text = command.Data.Options.First().Value.ToString();

        await command.RespondAsync("Odota hetki, prosessoin viestiäsi...");

        try
        {
            var response = await _groqClient.GetChatResponseAsync(text);
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Yksinkertaistettu: {response}");
        }
        catch (Exception ex)
        {
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"API-virhe: {ex.Message}");
        }
    }
}
