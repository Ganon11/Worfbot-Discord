using Newtonsoft.Json;

namespace Worfbot.WeatherModels
{
#pragma warning disable CS8618
  public class Rain
  {
    [JsonProperty("1h")]
    public double Precipitation { get; set;}
  }
#pragma warning restore CS8618
}
