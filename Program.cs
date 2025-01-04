using Discord;
using Discord.WebSocket;
using LanguageHelperApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(projectDirectory, @"..\..\.."))
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddSingleton<Bot>()
            .BuildServiceProvider();

        var bot = services.GetRequiredService<Bot>();

        // Käynnistä Bot
        await bot.StartAsync();
    }
}

