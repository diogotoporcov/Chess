using Chess.Core.Games;

namespace Chess.Core.Movement;

public interface IMoveExecutionResolver
{
    MoveExecution Resolve(
        GameState gameState,
        Move move);
}