using Newtonsoft.Json;

namespace Worfbot.Weather.WeatherModels
{
#pragma warning disable CS8618
  public class Weather
  {
    [JsonProperty("id")]
    public int Id { get;set; }

    [JsonProperty("main")]
    public string Main { get; set; }

  [JsonProperty("description")]
  public string Description { get; set; }

  [JsonProperty("icon")]
  public string IconCode { get; set; }
}
#pragma warning restore CS8618
}
