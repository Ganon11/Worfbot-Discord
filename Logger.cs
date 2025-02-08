using Discord;
using Discord.Commands;
using Discord.WebSocket;


namespace Worfbot
{
  public interface ILogger
  {
    public Task Log(LogMessage msg);
  }

  public class Logger : ILogger
  {
    public LogSeverity Severity { get; private set; }

    public Logger(string severity)
    {
      Severity = LogSeverity.Error;
      if (Enum.TryParse(typeof(LogSeverity), severity, out var parsedValue) && parsedValue != null)
      {
        Severity = (LogSeverity)parsedValue;
      }
    }

    public Logger(LogSeverity severity)
    {
      Severity = severity;
    }

    public Task Log(LogMessage msg)
    {
      if (msg.Severity <= Severity)
      {
        Console.WriteLine(msg.ToString());
      }

      return Task.CompletedTask;
    }
  }
}