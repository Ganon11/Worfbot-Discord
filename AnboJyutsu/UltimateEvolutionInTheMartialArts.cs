using Discord.Interactions;

namespace Worfbot.AnboJyutsu
{
  public enum Move
  {
    [ChoiceDisplay("Sweep the leg!")]
    Sweep = 1,
    [ChoiceDisplay("Duck beneath the blow!")]
    Duck = 2,
    [ChoiceDisplay("Strike your foe!")]
    Strike = 3
  }

  public enum Result
  {
    Loss = 0,
    Tie = 1,
    Victory = 2
  }

  public class UltimateEvolutionInTheMartialArtsUtilities
  {
    private static readonly Random R = new();

    public static Move GetRandomMove()
    {
      var moves = Enum.GetValues<Move>()!;
      return (Move)moves.GetValue(R.Next(moves.Length))!;
    }

    public static Result Evaluate(Move playerMove, Move worfbotMove)
    {
      if (playerMove == worfbotMove)
      {
        return Result.Tie;
      }

      if ((playerMove == Move.Sweep && worfbotMove == Move.Duck)
        || (playerMove == Move.Duck && worfbotMove == Move.Strike)
        || (playerMove == Move.Strike && worfbotMove == Move.Sweep))
      {
        return Result.Victory;
      }

      return Result.Loss;
    }
  }
}
