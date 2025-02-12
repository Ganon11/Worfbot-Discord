﻿using System.Text;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
      var collection = new ServiceCollection()
          .AddSingleton(new DiscordSocketConfig()
            {
              GatewayIntents = GatewayIntents.None
            })
          .AddSingleton(configuration)
          .AddSingleton<Logging.ILogger>(new Logging.Logger(configuration["Logging:Severity"] ?? ""))
          .AddSingleton<DiscordSocketClient>();

      _serviceProvider = collection.BuildServiceProvider();
    }

    public static Task Main(string[] args) => new Program().MainAsync(args);

    public async Task MainAsync(string[] args)
    {
      var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      client.Log += logger.Log;
      client.SlashCommandExecuted += SlashCommandHandler;
      // Uncomment when changing slash commands
      if (args.Any() && args[0].Equals("update-slash-commands", StringComparison.OrdinalIgnoreCase))
      {
        client.Ready += UpdateSlashCommands;
      }

      await client.LoginAsync(TokenType.Bot, configuration["TOKEN"]);
      await client.StartAsync();

      await Task.Delay(-1);
    }

    private async Task UpdateSlashCommands()
    {
      var client = _serviceProvider.GetRequiredService<DiscordSocketClient>();
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();

// #region /honor
      var honorCommand = new SlashCommandBuilder()
          .WithName("honor")
          .WithDescription("Checks whether the given topic is honorable.")
          .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to determine", isRequired: true);

      try
      {
        LogMessage message = new(LogSeverity.Info, nameof(UpdateSlashCommands), "Registering honor command...");
        await logger.Log(message);
        await client.CreateGlobalApplicationCommandAsync(honorCommand.Build());
      }
      catch (HttpException ex)
      {
        var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        LogMessage message = new(LogSeverity.Error, nameof(UpdateSlashCommands), json, ex);
        await logger.Log(message);
      }
// #endregion 

// #region /set-honor
      var setHonorCommand = new SlashCommandBuilder()
          .WithName("set-honor")
          .WithDescription("Informs Worfbot of the honorability of the topic.")
          .AddOption("topic", ApplicationCommandOptionType.String, "The word or phrase whose honor you want to set", isRequired: true)
          .AddOption("status", ApplicationCommandOptionType.Boolean, "True if the topic is honorable, false otherwise", isRequired: true);

      try
      {
        LogMessage message = new(LogSeverity.Info, nameof(UpdateSlashCommands), "Registering set-honor command...");
        await logger.Log(message);
        await client.CreateGlobalApplicationCommandAsync(setHonorCommand.Build());
      }
      catch (HttpException ex)
      {
        var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        LogMessage message = new(LogSeverity.Error, nameof(UpdateSlashCommands), json, ex);
        await logger.Log(message);
      }
// #endregion

// #region /weather
      var weatherCommand = new SlashCommandBuilder()
          .WithName("weather")
          .WithDescription("Asks Worfbot about the weather in a location.")
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("latitude")
            .WithDescription("Latitude, as a signed floating-point decimal")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.Number)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("longitude")
            .WithDescription("Longitude, as a signed floating-point decimal")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.Number)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("zip-code")
            .WithDescription("US ZIP code of location")
            .WithRequired(false)
            .WithMinLength(5)
            .WithMaxLength(5)
            .WithType(ApplicationCommandOptionType.String)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("city")
            .WithDescription("City Name")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.String)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("state")
            .WithDescription("State (2-Character String)")
            .WithRequired(false)
            .WithMinLength(2)
            .WithMaxLength(2)
            .WithType(ApplicationCommandOptionType.String)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("country")
            .WithDescription("Country Code (2-Character String)")
            .WithRequired(false)
            .WithMinLength(2)
            .WithMaxLength(2)
            .WithType(ApplicationCommandOptionType.String)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("units")
            .WithDescription("Which units to use? Default is Fahrenheit.")
            .WithRequired(false)
            .AddChoice("Kelvin", 1)
            .AddChoice("Fahrenheit", 2)
            .AddChoice("Celsius", 3)
            .WithType(ApplicationCommandOptionType.Integer)
          )
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("simple-display")
            .WithDescription("Display weather as simple text. Default is false.")
            .WithRequired(false)
            .WithType(ApplicationCommandOptionType.Boolean)
          );

      try
      {
        LogMessage message = new(LogSeverity.Info, nameof(UpdateSlashCommands), "Registering weather command...");
        await logger.Log(message);
        await client.CreateGlobalApplicationCommandAsync(weatherCommand.Build());
      }
      catch (HttpException ex)
      {
        var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        LogMessage message = new(LogSeverity.Error, nameof(UpdateSlashCommands), json, ex);
        await logger.Log(message);
      }
// #endregion

// #region /anbo-jyutsu
      var anboJyutsuCommand = new SlashCommandBuilder()
          .WithName("anbo-jyutsu")
          .WithDescription("Challenge Worfbot to a match of Anbo-Jyutsu. The ultimate evolution of the martial arts.")
          .AddOption(new SlashCommandOptionBuilder()
            .WithName("move")
            .WithDescription("What move do you use?")
            .WithRequired(true)
            .AddChoice("Sweep the legs!", 1)
            .AddChoice("Duck beneath the blow!", 2)
            .AddChoice("Strike your foe!", 3)
            .WithType(ApplicationCommandOptionType.Integer)
          );

      try
      {
        LogMessage message = new(LogSeverity.Info, nameof(UpdateSlashCommands), "Registering anbo-jyutsu command...");
        await logger.Log(message);
        await client.CreateGlobalApplicationCommandAsync(anboJyutsuCommand.Build());
      }
      catch (HttpException ex)
      {
        var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        LogMessage message = new(LogSeverity.Error, nameof(UpdateSlashCommands), json, ex);
        await logger.Log(message);
      }
// #endregion
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
        case "weather":
          await HandleWeatherCommand(command);
          break;
        case "anbo-jyutsu":
          await HandleAnboJyutsuCommand(command);
          break;
      }
    }

    private async Task HandleHonorCommand(SocketSlashCommand command)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      var option = command.Data.Options.FirstOrDefault();
      if (option == default)
      {
        await command.RespondAsync("You must specify a topic!", ephemeral: true);
        return;
      }

      var topic = option.Value.ToString();
      if (topic == null)
      {
        await command.RespondAsync("You must specify a topic!", ephemeral: true);
        return;
      }

      var honor = await Honor.Utilities.DetermineHonor(topic, configuration, logger);
      await command.RespondAsync(Honor.Utilities.FormatHonorResponse(topic, honor));

      LogMessage message = new(LogSeverity.Info, nameof(HandleHonorCommand), $"{command.User.Username} requested honor status of topic \"{topic}\" (result is {honor})");
      await logger.Log(message);

      return;
    }

    private async Task HandleSetHonorCommand(SocketSlashCommand command)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      var topicOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("topic"));
      if (topicOption == null)
      {
        await command.RespondAsync("You must specify a topic!", ephemeral: true);
        return;
      }

      var statusOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("status"));
      if (statusOption == null)
      {
        await command.RespondAsync("You must specify a status (true or false)!", ephemeral: true);
        return;
      }

      var topic = topicOption.Value.ToString();
      if (topic == null)
      {
        await command.RespondAsync("You must specify a topic!", ephemeral: true);
        return;
      }

      var statusString = statusOption.Value.ToString();
      if (statusString == null)
      {
        await command.RespondAsync("You must specify a status (true or false)!", ephemeral: true);
        return;
      }

      if (!bool.TryParse(statusString, out var status))
      {
        await command.RespondAsync("You must specify a status (true or false)!", ephemeral: true);
        return;
      }

      LogMessage message = new(LogSeverity.Info, nameof(HandleSetHonorCommand), $"{command.User.Username} setting honor status of topic \"{topic}\" to {status}");
      await logger.Log(message);

      await Honor.Utilities.SetHonor(topic, status, configuration);

      await command.RespondAsync($"{topic}'s honor status has been set to {status}.", ephemeral: true);
      return;
    }

    private async Task HandleWeatherCommand(SocketSlashCommand command)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      Weather.WeatherModels.Location? location = null;

      var zipCodeOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("zip-code"));
      if (zipCodeOption != null)
      {
        location = await Weather.API.GetLocationFromZip(zipCodeOption.Value.ToString()!, configuration, logger);
      }

      var cityOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("city"));
      if (cityOption != null)
      {
        var stateOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("state"));
        var countryOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("country"));
        string country = countryOption?.Value.ToString() ?? "US";
        location = await Weather.API.GetLocationFromCityName(cityOption.Value.ToString()!, state: stateOption?.Value.ToString(), country: country, configuration: configuration, logger: logger);
      }

      var latOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("latitude"));
      var lonOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("longitude"));
      if (latOption != null && lonOption != null)
      {
        location = new Weather.WeatherModels.Location { Latitude = Convert.ToDouble(latOption.Value), Longitude = Convert.ToDouble(lonOption.Value) };
      }

      if (location == null)
      {
        StringBuilder error = new();
        error.AppendLine("You must specify at least one way to determine the location!");
        error.AppendLine("Choose one of the following:");
        error.AppendLine("- ZIP Code");
        error.AppendLine("- Latitude and Longitude");
        error.AppendLine("- City Name");
        error.AppendLine("  - (Optional) State");
        error.AppendLine("  - (Optional) Country Code (Defaults to US)");
        await command.RespondAsync(error.ToString(), ephemeral: true);
        return;
      }

      Weather.Units units;
      var unitsOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("units"));
      units = unitsOption == null ? Weather.Units.Imperial : (Weather.Units)Convert.ToInt32(unitsOption.Value);

      LogMessage message = new(LogSeverity.Info, nameof(HandleWeatherCommand), $"{command.User.Username} requested weather for location \"{location}\", units \"{units}\"");
      await logger.Log(message);

      var prediction = await Weather.API.CheckWeather(location, units, configuration, logger);

      bool isSimpleDisplay = false;
      var displayOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("simple-display"));
      if (displayOption != null && bool.TryParse(displayOption.Value.ToString(), out bool parsedDisplay))
      {
        isSimpleDisplay = parsedDisplay;
      }

      if (isSimpleDisplay)
      {
        StringBuilder response = new();
        response.AppendLine($"Weather for {prediction.Location}");
        response.AppendLine(Weather.Formatting.FormatWeatherPrediction(prediction, units));
        await command.RespondAsync(text: response.ToString());
      }
      else
      {
        var embedBuilder = new EmbedBuilder()
          .WithTitle($"Weather for {prediction.Location}")
          .WithThumbnailUrl($"https://openweathermap.org/img/wn/{prediction.WeatherForecasts.First().IconCode}@2x.png")
          .WithDescription(Weather.Formatting.FormatWeatherPrediction(prediction, units))
          .WithCurrentTimestamp();

        await command.RespondAsync(embed: embedBuilder.Build());
      }

      return;
    }

    private async Task HandleAnboJyutsuCommand(SocketSlashCommand command)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();

      var moveOption = command.Data.Options.FirstOrDefault(o => o.Name.Equals("move"));
      if (moveOption == null)
      {
        await command.RespondAsync("You must be decisive in anbo-jyutsu! Make a move!", ephemeral: true);
        return;
      }

      AnboJyutsu.Move playerMove = (AnboJyutsu.Move)Convert.ToInt32(moveOption.Value);
      AnboJyutsu.Move myMove = AnboJyutsu.UltimateEvolutionInTheMartialArtsUtilities.GetRandomMove();
      StringBuilder response = new();
      response.AppendLine("Yoroshiku onegaishimasu!");
      response.AppendLine($"You selected {playerMove.ToString()}, whereas I have selected {myMove.ToString()}.");
      switch (AnboJyutsu.UltimateEvolutionInTheMartialArtsUtilities.Evaluate(playerMove, myMove))
      {
        case AnboJyutsu.Result.Tie:
          response.AppendLine("It seems we are evenly matched!");
          break;
        case AnboJyutsu.Result.Victory:
          response.AppendLine("You have defeated me!");
          break;
        case AnboJyutsu.Result.Loss:
          response.AppendLine("Victory is mine!");
          break;
      }

      await command.RespondAsync(response.ToString());
      return;
    }
  }
}
