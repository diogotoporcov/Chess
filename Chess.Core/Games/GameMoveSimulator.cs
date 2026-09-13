using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class GameMoveSimulator
{
    private readonly IMoveExecutionResolver _executionResolver;

    public GameMoveSimulator(
        IMoveExecutionResolver executionResolver)
    {
        ArgumentNullException.ThrowIfNull(executionResolver);

        _executionResolver = executionResolver;
    }

    public TResult Evaluate<TResult>(
        GameState gameState,
        Move move,
        Func<GameState, MoveExecution, TResult> evaluator)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(evaluator);

        var execution = _executionResolver.Resolve(gameState, move);

        if (execution.Move != move)
        {
            throw new InvalidOperationException(
                "Move execution resolver returned an execution for a different move.");
        }

        gameState.BoardState.ApplyTransition(execution.Transition);

        try
        {
            return evaluator(gameState, execution);
        }
        finally
        {
            gameState.BoardState.RevertTransition(execution.Transition);
        }
    }
}
