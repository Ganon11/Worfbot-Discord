using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Worfbot
{
  public class Program
  {
    private readonly IServiceProvider _serviceProvider;

    public Program()
    {
      IConfiguration configuration = new ConfigurationBuilder()
        .AddEnvironmentVariables(prefix: "DATABASE_")
        .AddEnvironmentVariables(prefix: "DISCORD_BOT_")
        .AddEnvironmentVariables(prefix: "WEATHER_")
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true)
        .Build();
      _serviceProvider = new ServiceCollection()
        .AddSingleton(new DiscordSocketConfig()
          {
            GatewayIntents = GatewayIntents.None
          })
        .AddSingleton(configuration)
        .AddSingleton<Logging.ILogger>(new Logging.Logger(configuration["Logging:Severity"] ?? ""))
        .AddSingleton<DiscordSocketClient>()
        .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
        .BuildServiceProvider();
    }

    public static Task Main(string[] args) => new Program().MainAsync(args);

    public async Task MainAsync(string[] args)
    {
      var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      client.Log += logger.Log;
      client.Ready += RegisterCommands;

      await client.LoginAsync(TokenType.Bot, configuration["TOKEN"]);
      await client.StartAsync();

      await Task.Delay(-1);
    }

    private async Task RegisterCommands()
    {
      var interactionService = _serviceProvider.GetRequiredService<InteractionService>();
      await interactionService.RegisterCommandsGloballyAsync();
    }
  }
}
