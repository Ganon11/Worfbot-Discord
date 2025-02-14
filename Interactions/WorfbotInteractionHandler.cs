using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Worfbot.Interactions
{
  public class WorfbotInteractionHandler
  {
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _service;
    private readonly IServiceProvider _provider;

    public WorfbotInteractionHandler(DiscordSocketClient client, InteractionService service, IServiceProvider provider)
    {
      _client = client;
      _service = service;
      _provider = provider;
    }

    public async Task InitializeAsync()
    {
      var logger = _provider.GetRequiredService<Logging.ILogger>();
      await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(WorfbotInteractionHandler), "Initializing interaction handler"));
      _client.Ready += ReadyAsync;
      _client.InteractionCreated += async interaction => {
        await _provider.GetService<Logging.ILogger>()!.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, nameof(WorfbotInteractionHandler), "Handling interaction"));
        var context = new SocketInteractionContext(_client, interaction);
        var result = await _service.ExecuteCommandAsync(context, _provider);
      };
      _service.SlashCommandExecuted += async (command, context, result) => {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, nameof(WorfbotInteractionHandler), "Slash Command Executed"));
      };
    }

    private async Task ReadyAsync()
    {
      await _provider.GetService<Logging.ILogger>()!.Log(new Discord.LogMessage(Discord.LogSeverity.Debug, nameof(WorfbotInteractionHandler), "Registering commands..."));
      await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
      await _service.RegisterCommandsGloballyAsync();
    }
  }
}
