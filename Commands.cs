using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
            if (option == null)
            {
                await command.ModifyOriginalResponseAsync(msg =>
                    msg.Content = "Ole hyvä ja liitä kuva tai anna kuvan URL-osoite.");
                return;
            }

            byte[] imageBytes;
            string fileName;

            if (option.Value is IAttachment attachment)
            {
                if (!attachment.ContentType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ?? true)
                {
                    await command.ModifyOriginalResponseAsync(msg =>
                        msg.Content = "Vain kuvatiedostot ovat sallittuja.");
                    return;
                }

                fileName = attachment.Filename;
                using var httpClient = new HttpClient();
                imageBytes = await httpClient.GetByteArrayAsync(attachment.Url);
            }

            else if (option.Value is string imageUrl)
            {
                if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != "http" && uri.Scheme != "https"))
                {
                    await command.ModifyOriginalResponseAsync(msg =>
                        msg.Content = "Antamasi URL-osoite ei ole kelvollinen. Varmista, että se alkaa 'http://' tai 'https://'.");
                    return;
                }

                fileName = "image_from_url.jpg";
                using var httpClient = new HttpClient();
                imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
            }
            else
            {
                await command.ModifyOriginalResponseAsync(msg =>
                    msg.Content = "Virheellinen syöte. Ole hyvä ja liitä kuva tai anna kuvan URL-osoite.");
                return;
            }

            var response = await _groqClient.GetAnalyzeImageAsync(imageBytes, fileName);
            await command.ModifyOriginalResponseAsync(msg => msg.Content = $"Kuvan tulkinta: {response}");
        }
        catch (Exception ex)
        {
            await command.ModifyOriginalResponseAsync(msg =>
                msg.Content = $"Virhe kuvan käsittelyssä: {ex.Message}");
        }
    }
}
