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
    private readonly CommandService _commands;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _services;

    public Bot(IConfiguration configuration)
    {
        _configuration = configuration;
        _client = new DiscordSocketClient();
        _commands = new CommandService();

        // Palveluntarjoaja
        _services = ConfigureServices();

        // Alustetaan tapahtumat
        _client.Log += LogAsync;
        _client.MessageReceived += HandleCommandAsync;
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

        // Rekisteröidään komennot
        await RegisterCommandsAsync();

        // Estetään ohjelman sulkeutuminen
        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task RegisterCommandsAsync()
    {
        // Rekisteröidään komennot CommandsModule-luokasta
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Varmistetaan, että viesti on käyttäjän viesti, ei botin
        if (messageParam is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;

        // Määritetään komennon konteksti
        var context = new SocketCommandContext(_client, message);

        int argPos = 0;
        // Tarkistetaan, onko viesti komento (!-etuliitteellä)
        if (message.HasCharPrefix('!', ref argPos))
        {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                Console.WriteLine($"Komento epäonnistui: {result.ErrorReason}");
            }
        }
    }

    private IServiceProvider ConfigureServices()
    {
        // Rekisteröidään riippuvuudet
        return new ServiceCollection()
            .AddSingleton(_configuration)                     // Lisää asetukset
            .AddSingleton(_client)                            // Lisää DiscordSocketClient
            .AddSingleton(_commands)                          // Lisää CommandService
            .AddSingleton<GroqAPI>()                       // Lisää GroqClient
            .BuildServiceProvider();
    }
}
