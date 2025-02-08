using Newtonsoft.Json;

namespace Worfbot.WeatherModels
{
#pragma warning disable CS8618
  public class Wind
  {
    [JsonProperty("speed")]
    public double WindSpeed { get; set; }

    [JsonProperty("deg")]
    public double Degrees { get; set; }

    [JsonProperty("gust")]
    public double Gust { get; set; }
  }
#pragma warning restore CS8618
}
