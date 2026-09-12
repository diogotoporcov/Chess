using Chess.Core.Games;

namespace Chess.Core.Movement;

public interface IMoveExecutionResolver
{
    bool CanResolve(
        Move move);

    MoveExecution Resolve(
        GameState gameState,
        Move move);
}