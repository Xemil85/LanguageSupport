using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

public class Bot
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    public Bot(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
        });

        _services = ConfigureServices();

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.SlashCommandExecuted += HandleSlashCommandAsync;
    }

    public async Task StartAsync()
    {
        // Haetaan bot-token
        string token = _configuration["DiscordBot:Token"];
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Token puuttuu! Tarkista appsettings.json.");
            return;
        }

        // Käynnistetään botti
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Estetään ohjelman sulkeutuminen
        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync()
    {
        Console.WriteLine("Bot is ready!");

        var simplifyCommand = new SlashCommandBuilder()
            .WithName("simplify")
            .WithDescription("Yksinkertaistaa annetun tekstin GroqCloud-rajapinnan avulla.")
            .AddOption("text", ApplicationCommandOptionType.String, "Muunna teksti yksinkertaisempaan muotoon", isRequired: true);

        var wordCommand = new SlashCommandBuilder()
            .WithName("word")
            .WithDescription("Kertoo annetun sanan merkityksen GroqCloud-rajapinnan avulla.")
            .AddOption("word", ApplicationCommandOptionType.String, "Kerro sanan merkitys", isRequired: true);

        try
        {
            await _client.CreateGlobalApplicationCommandAsync(simplifyCommand.Build());
            await _client.CreateGlobalApplicationCommandAsync(wordCommand.Build());
            Console.WriteLine("Slash-komennot rekisteröity onnistuneesti!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Virhe rekisteröinnissä: {ex.Message}");
        }
    }

    private async Task HandleSlashCommandAsync(SocketSlashCommand command)
    {
        using var scope = _services.CreateScope();
        var commandsHandler = scope.ServiceProvider.GetRequiredService<Commands>();

        await commandsHandler.HandleSlashCommandAsync(command);
    }

    private IServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(_configuration)
            .AddSingleton(_client)
            .AddSingleton<GroqAPI>()
            .AddSingleton<Commands>()
            .BuildServiceProvider();
    }
}
