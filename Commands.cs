using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

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

            case "word":
                await HandleWordCommand(command);
                break;

            case "image":
                var option = command.Data.Options.FirstOrDefault();
                if (option == null)
                {
                    await command.RespondAsync("Ole hyvä ja liitä kuva tai anna kuvan URL-osoite.");
                    return;
                }

                if (option.Name == "image")
                {
                    var attachment = option.Value as IAttachment;
                    await HandleImageCommand(command);
                }
                else if (option.Name == "url")
                {
                    var url = option.Value as string;
                    await HandleImageCommand(command);
                }
                else
                {
                    await command.RespondAsync("Virheellinen syöte. Ole hyvä ja liitä kuva tai anna kuvan URL-osoite.");
                }
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

    private async Task HandleWordCommand(SocketSlashCommand command)
    {
        var word = command.Data.Options.First().Value.ToString();
        await command.RespondAsync("Odota hetki, prosessoin viestiäsi...");

        try
        {
            var response = await _groqClient.GetWordResponseAsync(word);
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Sanan merkitys: {response}");
        } catch (Exception ex)
        {
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"API-virhe: {ex.Message}");
        }
    }

    private async Task HandleImageCommand(SocketSlashCommand command)
    {
        await command.RespondAsync("Odota hetki, prosessoin viestiäsi...");

        try
        {
            var option = command.Data.Options.FirstOrDefault();
            if (option?.Value is not IAttachment attachment)
            {
                await command.ModifyOriginalResponseAsync(msg =>
                    msg.Content = "Ole hyvä ja liitä kuva käsittelyä varten.");
                return;
            }

            // Tarkista, että tiedosto on kuvatyyppiä
            if (!attachment.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                await command.ModifyOriginalResponseAsync(msg =>
                    msg.Content = "Vain kuvatiedostot ovat sallittuja.");
                return;
            }

            var fileName = attachment.Filename;

            // Lataa kuvan data
            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(attachment.Url);

            if (imageBytes == null || imageBytes.Length == 0)
            {
                await command.ModifyOriginalResponseAsync(msg =>
                    msg.Content = "Kuvatiedoston lataaminen epäonnistui.");
                return;
            }

            // Lähetä kuva API:lle analysoitavaksi
            var response = await _groqClient.GetAnalyzeImageAsync(imageBytes, fileName);

            // Tarkista, onko API-vastaus liian pitkä Discord-viestiksi
            if (response.Length > 2000)
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                await command.ModifyOriginalResponseAsync(msg =>
                    msg.Attachments = new[] { new FileAttachment(stream, "analyysi.txt") });
            }
            else
            {
                await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Kuvan tulkinta: {response}");
            }
        }
        catch (Exception ex)
        {
            await command.ModifyOriginalResponseAsync(msg =>
                msg.Content = $"Virhe kuvan käsittelyssä: {ex.Message}");
        }
    }
}
