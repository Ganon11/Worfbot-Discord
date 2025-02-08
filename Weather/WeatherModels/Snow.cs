using Newtonsoft.Json;

namespace Worfbot.Weather.WeatherModels
{
#pragma warning disable CS8618
  public class Snow
  {
    [JsonProperty("1h")]
    public double Precipitation { get; set;}
  }
#pragma warning restore CS8618
}
