using Microsoft.Extensions.Configuration;

namespace Worfbot.Honor
{
  public enum HonorStatus
  {
    Dishonorable = 0,
    Honorable = 1,
    Unknown = 2
  }

  public static class Utilities
  {
    private static string CreateMD5(string input)
    {
      byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
      byte[] hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);

      return Convert.ToHexString(hashBytes);
    }

    private static readonly char[] HONORABLE_SUFFIXES = new char[] { '0', '1', '2', '3', '4', '5', '6', '7' };

    public static async Task<bool> DetermineHonor(string topic, IConfiguration configuration, Logging.ILogger? logger = null)
    {
      var database = new Database.Database(configuration);
      var databaseStatus = await database.DetermineHonorFromDatabase(topic);
      if (databaseStatus != HonorStatus.Unknown)
      {
        if (logger != null)
        {
          await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(DetermineHonor), $"Honor determined from database"));
        }
        return databaseStatus == HonorStatus.Honorable;
      }

      var md5 = CreateMD5(topic);
      if (logger != null)
      {
        await logger.Log(new Discord.LogMessage(Discord.LogSeverity.Info, nameof(DetermineHonor), $"Honor determined from MD5 hash ({md5})"));
      }
      return HONORABLE_SUFFIXES.Contains(md5.Last());
    }

    public static async Task SetHonor(string topic, bool status, IConfiguration configuration)
    {
      var database = new Database.Database(configuration);
      await database.SetHonorInDatabase(topic, status);
    }
  }
}