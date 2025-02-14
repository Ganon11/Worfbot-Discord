using System.Text;
using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Worfbot.Interactions
{
  public class WorfbotInteractionModule : InteractionModuleBase<SocketInteractionContext>
  {
    private readonly IServiceProvider _serviceProvider;

    public WorfbotInteractionModule(IServiceProvider serviceProvider)
    {
      _serviceProvider = serviceProvider;
    }

    [SlashCommand("echo", "Echo an input")]
    public async Task Echo(string input)
    {
      await RespondAsync(input);
    }

    [SlashCommand("honor", "Checks whether the given topic is honorable.")]
    public async Task HonorCommand(string topic)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      var honor = await Honor.Utilities.DetermineHonor(topic, configuration, logger);
      await RespondAsync(Honor.Utilities.FormatHonorResponse(topic, honor));

      LogMessage message = new(LogSeverity.Info, nameof(Honor), $"{Context.User.Username} requested honor status of topic \"{topic}\" (result is {honor})");
      await logger.Log(message);

      return;
    }

    [SlashCommand("set-honor", "Informs Worfbot of the honorability of the topic.")]
    public async Task SetHonorCommand(string topic, bool status)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      LogMessage message = new(LogSeverity.Info, nameof(SetHonorCommand), $"{Context.User.Username} setting honor status of topic \"{topic}\" to {status}");
      await logger.Log(message);

      await Honor.Utilities.SetHonor(topic, status, configuration);

      await RespondAsync($"{topic}'s honor status has been set to {status}.", ephemeral: true);
      return;
    }

    [SlashCommand("weather", "Asks Worfbot about the weather in a location.")]
    public async Task WeatherCommand(double? latitude = null, double? longitude = null, string? zipCode = null,
      string? city = null, string? state = null, string? country = null, Weather.Units units = Weather.Units.Imperial,
      bool simpleDisplay = false)
    {
      var logger = _serviceProvider.GetRequiredService<Logging.ILogger>();
      var configuration = _serviceProvider.GetRequiredService<IConfiguration>();

      Weather.WeatherModels.Location? location = null;

      if (!string.IsNullOrEmpty(zipCode))
      {
        location = await Weather.API.GetLocationFromZip(zipCode!, configuration, logger);
      }
      else if (!string.IsNullOrEmpty(city))
      {
        country ??= "US";
        location = await Weather.API.GetLocationFromCityName(city!, state: state, country: country, configuration: configuration, logger: logger);
      }
      else if (latitude.HasValue && longitude.HasValue)
      {
        location = new Weather.WeatherModels.Location { Latitude = latitude.Value, Longitude = longitude.Value };
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
        await RespondAsync(error.ToString(), ephemeral: true);
        return;
      }

      LogMessage message = new(LogSeverity.Info, nameof(WeatherCommand), $"{Context.User.Username} requested weather for location \"{location}\", units \"{units}\"");
      await logger.Log(message);

      var prediction = await Weather.API.CheckWeather(location, units, configuration, logger);

      if (simpleDisplay)
      {
        StringBuilder response = new();
        response.AppendLine($"Weather for {prediction.Location}");
        response.AppendLine(Weather.Formatting.FormatWeatherPrediction(prediction, units));
        await RespondAsync(text: response.ToString());
      }
      else
      {
        var embedBuilder = new EmbedBuilder()
          .WithTitle($"Weather for {prediction.Location}")
          .WithThumbnailUrl($"https://openweathermap.org/img/wn/{prediction.WeatherForecasts.First().IconCode}@2x.png")
          .WithDescription(Weather.Formatting.FormatWeatherPrediction(prediction, units))
          .WithCurrentTimestamp();

        await RespondAsync(embed: embedBuilder.Build());
      }

      return;
    }

    [SlashCommand("anbo-jyutsu", "Challenge Worfbot to a match of Anbo-Jyutsu. The ultimate evolution of the martial arts.")]
    public async Task AnboJyutsuCommand(AnboJyutsu.Move move)
    {
      AnboJyutsu.Move myMove = AnboJyutsu.UltimateEvolutionInTheMartialArtsUtilities.GetRandomMove();
      StringBuilder response = new();
      response.AppendLine("Yoroshiku onegaishimasu!");
      response.AppendLine($"You selected {move}, whereas I have selected {myMove}.");
      switch (AnboJyutsu.UltimateEvolutionInTheMartialArtsUtilities.Evaluate(move, myMove))
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

      await RespondAsync(response.ToString());
      return;
    }
  }
}
