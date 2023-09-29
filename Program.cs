using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pluralize.NET;

namespace Ganon11.Worfbot
{
  public class Program
  {
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public Program()
    {
      _configuration = new ConfigurationBuilder()
         .AddEnvironmentVariables(prefix: "DATABASE_")
         .AddEnvironmentVariables(prefix: "DISCORD_BOT_")
         .AddJsonFile("appsettings.json", optional: true)
         .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly(), optional: true)
         .Build();

      var discordConfig = new DiscordSocketConfig()
      {
        GatewayIntents = GatewayIntents.None
      };

      var collection = new ServiceCollection()
         .AddSingleton(discordConfig)
         .AddSingleton(_configuration)
         .AddSingleton<DiscordSocketClient>();

      _serviceProvider = collection.BuildServiceProvider();
    }

    public static Task Main(string[] args) => new Program().MainAsync(args);

    public async Task MainAsync(string[] args)
    {
      if (args.Any() && args[0].Equals("update-slash-commands", StringComparison.OrdinalIgnoreCase))
      {
        await UpdateSlashCommands();
        return;
      }

      var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

      client.Log += Log;
      client.SlashCommandExecuted += SlashCommandHandler;

      await client.LoginAsync(TokenType.Bot, _configuration["TOKEN"]);
      await client.StartAsync();

      await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
      LogSeverity severity = LogSeverity.Error;
      if (Enum.TryParse(typeof(LogSeverity), _configuration["Logging:Severity"], out var parsedValue) && parsedValue != null)
      {
        severity = (LogSeverity)parsedValue;
      }

      if (msg.Severity <= severity)
      {
        Console.WriteLine(msg.ToString());
      }
      
      return Task.CompletedTask;
    }

    private async Task UpdateSlashCommands()
    {
      var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
      var honorCommand = new SlashCommandBuilder()
         .WithName("honor")
         .WithDescription("Checks whether the given topic is honorable.")
         .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to determine", isRequired: true);

      try
      {
        await client.CreateGlobalApplicationCommandAsync(honorCommand.Build());
      }
      catch (HttpException ex)
      {
        var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        LogMessage message = new(LogSeverity.Error, nameof(UpdateSlashCommands), json);
        await Log(message);
      }

      var setHonorCommand = new SlashCommandBuilder()
         .WithName("set-honor")
         .WithDescription("Informs Worfbot of the honorability of the topic.")
         .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to set", isRequired: true)
         .AddOption("status", ApplicationCommandOptionType.Boolean, "True if the topic is honorable, false otherwise", isRequired: true);

      try
      {
        await client.CreateGlobalApplicationCommandAsync(setHonorCommand.Build());
      }
      catch (HttpException ex)
      {
        var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        LogMessage message = new(LogSeverity.Error, nameof(UpdateSlashCommands), json);
        await Log(message);
      }
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
      switch (command.Data.Name)
      {
        case "honor":
          await HandleHonorCommand(command);
          break;
        case "set-honor":
          await HandleSetHonorCommand(command);
          break;
      }
    }

    private async Task<bool> IsPlural(string topic)
    {
      LogMessage message = new(LogSeverity.Debug, nameof(IsPlural), $"Checking pluralization of {topic}");
      await Log(message);
      IPluralize pluralizer = new Pluralizer();
      return pluralizer.IsPlural(topic);
    }

    private async Task HandleHonorCommand(SocketSlashCommand command)
    {
      var option = command.Data.Options.FirstOrDefault();
      if (option == default)
      {
        return;
      }

      var topic = option.Value.ToString();
      if (topic == null)
      {
        return;
      }

      LogMessage message = new(LogSeverity.Info, nameof(HandleHonorCommand), $"{command.User.Username} requested honor status of topic \"{topic}\"");
      await Log(message);
      var honor = await HonorUtilities.DetermineHonor(topic, _configuration);
      if (honor)
      {
        var verb = await IsPlural(topic) ? "have" : "has";
        await command.RespondAsync($"{topic} {verb} honor.");
      }
      else
      {
        var verb = await IsPlural(topic) ? "are" : "is";
        await command.RespondAsync($"{topic} {verb} without honor.");
      }

      return;
    }

    private async Task HandleSetHonorCommand(SocketSlashCommand command)
    {
      var topicOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("topic"));
      if (topicOption == null)
      {
        return;
      }

      var statusOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("status"));
      if (statusOption == null)
      {
        return;
      }

      var topic = topicOption.Value.ToString();
      if (topic == null)
      {
        return;
      }

      var statusString = statusOption.Value.ToString();
      if (statusString == null)
      {
        return;
      }

      if (!bool.TryParse(statusString, out var status))
      {
        return;
      }

      LogMessage message = new(LogSeverity.Info, nameof(HandleSetHonorCommand), $"{command.User.Username} setting honor status of topic \"{topic}\" to {status}");
      await Log(message);

      await HonorUtilities.SetHonor(topic, status, _configuration);

      await command.RespondAsync($"{topic}'s honor status has been set to {status}.", ephemeral: true);
      return;
    }
  }
}
