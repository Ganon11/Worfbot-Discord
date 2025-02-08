using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Worfbot.Database
{
  public class Database
  {
    private readonly string _username;
    private readonly string _password;
    private readonly string _host;
    private readonly string _name;

    private readonly string _connectionString;

    public Database(IConfiguration configuration)
    {
      _username = configuration["USERNAME"] ?? "";
      _password = configuration["PASSWORD"] ?? "";
      _host = configuration["HOST"] ?? "";
      _name = configuration["NAME"] ?? "";

      _connectionString = $"Host={_host};Username={_username};Password={_password};Database={_name};SSL Mode=Disable";
    }

    private NpgsqlConnection ConnectToDatabase()
    {
      var conn = new NpgsqlConnection(_connectionString);

      return conn;
    }

    public async Task<Honor.HonorStatus> DetermineHonorFromDatabase(string topic)
    {
      await using var conn = ConnectToDatabase();
      await conn.OpenAsync();

      await using var selectCommand = new NpgsqlCommand("SELECT status FROM honorable WHERE topic = ($1)", conn)
      {
        Parameters =
        {
          new() { Value = topic }
        }
      };

      var result = await selectCommand.ExecuteScalarAsync();
#pragma warning disable IDE0046 // Convert to conditional expression
      if (result == null)
      {
        return Honor.HonorStatus.Unknown;
      }
#pragma warning restore IDE0046 // Convert to conditional expression

      return (bool)result ? Honor.HonorStatus.Honorable : Honor.HonorStatus.Dishonorable;
    }

    public async Task SetHonorInDatabase(string topic, bool status)
    {
      await using var conn = ConnectToDatabase();
      await conn.OpenAsync();

      await using var selectCommand = new NpgsqlCommand("SELECT status FROM honorable WHERE topic = ($1)", conn)
      {
        Parameters =
        {
          new() { Value = topic }
        }
      };

      var result = await selectCommand.ExecuteScalarAsync();
      if (result == null)
      {
        await using var insertCommand = new NpgsqlCommand("INSERT INTO honorable(topic, status) VALUES (($1), ($2))", conn)
        {
          Parameters =
          {
            new() { Value = topic },
            new() { Value = status }
          }
        };

        await insertCommand.ExecuteNonQueryAsync();
      }
      else
      {
        await using var updateCommand = new NpgsqlCommand("UPDATE honorable SET status = ($2) WHERE topic = ($1)", conn)
        {
          Parameters =
          {
            new() { Value = topic },
            new() { Value = status }
          }
        };

        await updateCommand.ExecuteNonQueryAsync();
      }
    }
  }
}
