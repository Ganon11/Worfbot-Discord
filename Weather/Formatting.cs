using System.Text;
using Worfbot.Weather.WeatherModels;

namespace Worfbot.Weather
{
  public static class Formatting
  {
    private static string FormatPrecipitation(Rain rain)
    {
      return $"☔ {rain.Precipitation} mm/h";
    }

    private static string FormatPrecipitation(Snow snow)
    {
      return $"❄️ {snow.Precipitation} mm/h";
    }

    private static string FormatDegrees(double degrees, Units units)
    {
      return units switch
      {
        Units.Standard => $"{degrees} K",
        Units.Imperial => $"{degrees}°F",
        Units.Metric => $"{degrees}°C",
        _ => $"{degrees}°F",
      };
    }

    private const double TEMP_EPSILON = 0.01;

    private static string FormatTemperatures(Temperatures temps, Units units)
    {
      StringBuilder stringBuilder = new();
      stringBuilder.Append(FormatDegrees(temps.Temperature, units));
      stringBuilder.Append($" (between {FormatDegrees(temps.MinTemp, units)} and {FormatDegrees(temps.MaxTemp, units)})");
      if (Math.Abs(temps.Temperature - temps.FeelsLike) >= TEMP_EPSILON)
      {
        stringBuilder.Append($", 🍃 Feels Like {FormatDegrees(temps.FeelsLike, units)}");
      }
      return stringBuilder.ToString();
    }

    private static string FormatWindSpeed(double speed, Units units)
    {
      return units == Units.Metric ? $"{speed} m/s" : $"{speed} mph";
    }

    private static string FormatWindDirection(double degrees)
    {
      if (degrees < 0 || degrees > 360)
      {
        throw new Exception("Invalid degrees!");
      }

      if (degrees < 11.25)
      {
        return "⬆️ North";
      }

      if (degrees < 33.75)
      {
        return "⬆️ North-Northeast";
      }

      if (degrees < 56.25)
      {
        return "↗️ Northeast";
      }

      if (degrees < 78.75)
      {
        return "↗️ East-Northeast";
      }

      if (degrees < 101.25)
      {
        return "➡️ East";
      }

      if (degrees < 123.75)
      {
        return "➡️ East-Southeast";
      }

      if (degrees < 146.25)
      {
        return "↘️ Southeast";
      }

      if (degrees < 168.75)
      {
        return "↘️ South-Southeast";
      }

      if (degrees < 191.25)
      {
        return "⬇️ South";
      }

      if (degrees < 213.75)
      {
        return "⬇️ South-Southwest";
      }

      if (degrees < 236.25)
      {
        return "↙️ Southwest";
      }

      if (degrees < 258.75)
      {
        return "↙️ West-Southwest";
      }

      if (degrees < 281.25)
      {
        return "⬅️ West";
      }

      if (degrees < 303.75)
      {
        return "⬅️ West-Northwest";
      }

      if (degrees < 326.25)
      {
        return "↖️ Northwest";
      }

      if (degrees < 348.75)
      {
        return "↖️ North-Northwest";
      }

      return "⬆️ North";
    }

    private static string FormatWind(Wind wind, Units units)
    {
      return $"{FormatWindSpeed(wind.WindSpeed, units)} {FormatWindDirection(wind.Degrees)}, gusts of {FormatWindSpeed(wind.Gust, units)}";
    }

    private static string GetWeatherEmoji(int id)
    {
      // Thunderstorm
      if (200 <= id && id <= 232)
      {
        return "⛈️";
      }

      // Drizzle
      if (300 <= id && id <= 321)
      {
        return "☔";
      }

      // Rain
      if (500 <= id && id <= 531)
      {
        return "🌧️";
      }

      // Snow
      if (600 <= id && id <= 622)
      {
        return "🌨️";
      }

      // Atmosphere (???)
      if (700 <= id && id < 781)
      {
        return "🌫️";
      }

      // Clear, or Clouds
      return id switch
      {
        800 => "😎",
        801 => "🌤️",
        802 => "⛅",
        _ => "☁️",
      };
    }

    private static string GetWeatherBriefSummary(WeatherPrediction prediction)
    {
      List<string> individualConditions = new();
      foreach (WeatherModels.Weather weather in prediction.WeatherForecasts)
      {
        individualConditions.Add($"{GetWeatherEmoji(weather.Id)} {weather.Main}");
      }

      return string.Join(", ", individualConditions);
    }

    public static string FormatWeatherPrediction(WeatherPrediction prediction, Units units)
    {
      StringBuilder stringBuilder = new();
      stringBuilder.AppendLine(GetWeatherBriefSummary(prediction));
      if (prediction.Rain != null)
      {
        stringBuilder.AppendLine(FormatPrecipitation(prediction.Rain));
      }
      if (prediction.Snow != null)
      {
        stringBuilder.AppendLine(FormatPrecipitation(prediction.Snow));
      }
      stringBuilder.AppendLine($"🌡️ {FormatTemperatures(prediction.Temps, units)}");
      stringBuilder.AppendLine($"💨 {FormatWind(prediction.Wind, units)}");
      return stringBuilder.ToString();
    }
  }
}
