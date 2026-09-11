namespace Chess.Core.Games.Status;

public interface IGameStatusEvaluator
{
    GameStatus Evaluate(
        GameState gameState);
}