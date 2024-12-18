using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LanguageHelperApp
{
    public class Bot
    {
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly HttpClient _httpClient;

        public Bot(IConfiguration configuration)
        {
            _configuration = configuration;
            _client = new DiscordSocketClient();
            _httpClient = new HttpClient();
        }

        public async Task StartAsync()
        {
            // Lataa Discord token ja Groq API-avaimen appsettings.json:sta
            var discordToken = _configuration["DiscordBot:Token"];
            var groqApiKey = _configuration["GroqApi:Key"];
            var groqBaseUrl = _configuration["GroqApi:BaseUrl"];

            // Botin yhteys Discordiin
            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;

            // Yhdistä Discordiin
            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            // Estä ohjelman sulkeutuminen
            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }

        // Tämä tapahtuu, kun botti on valmis
        private Task ReadyAsync()
        {
            Console.WriteLine("Botti on valmis!");
            return Task.CompletedTask;
        }
    }
}
