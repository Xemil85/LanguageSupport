using Discord;
using Discord.WebSocket;
using LanguageHelperApp;
using Microsoft.Extensions.Configuration;

public class Program
{
    public static async Task Main(string[] args)
    {
        string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(projectDirectory, @"..\..\.."))
            .AddJsonFile("appsettings.json")
            .Build();

        var bot = new Bot(configuration);

        // Käynnistä Bot
        await bot.StartAsync();
    }
}

