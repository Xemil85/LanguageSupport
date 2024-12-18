using Discord.Commands;
using System.Threading.Tasks;

public class Commands : ModuleBase<SocketCommandContext>
{
    private readonly GroqAPI _groqClient;

    public Commands(GroqAPI groqClient)
    {
        _groqClient = groqClient;
    }

    [Command("simplify")]
    [Summary("Yksinkertaistaa annetun tekstin GroqCloud-rajapinnan avulla.")]
    public async Task SimplifyAsync([Remainder] string text)
    {
        await ReplyAsync("Odota hetki, prosessoin viestiäsi...");

        try
        {
            var response = await _groqClient.GetChatResponseAsync(text);
            await ReplyAsync($"Simplified: {response}");
        }
        catch (HttpRequestException ex)
        {
            await ReplyAsync($"API-virhe: {ex.Message}");
        }
    }
}
