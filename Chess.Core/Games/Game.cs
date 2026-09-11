using Chess.Core.Board;
using Chess.Core.Games.Status;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class Game
{
    private readonly IGameMoveGenerator _moveGenerator;

    private readonly GameMoveExecutor _moveExecutor;

    private readonly IGameStatusEvaluator _statusEvaluator;

    public GameVariantDefinition Variant { get; }

    public GameState State { get; }

    public BoardState BoardState =>
        State.BoardState;

    public GameStatus Status => _statusEvaluator.Evaluate(State);

    internal Game(
        GameVariantDefinition variant,
        GameState state,
        IGameMoveGenerator moveGenerator,
        GameMoveExecutor moveExecutor,
        IGameStatusEvaluator statusEvaluator)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(moveGenerator);
        ArgumentNullException.ThrowIfNull(moveExecutor);
        ArgumentNullException.ThrowIfNull(statusEvaluator);

        Variant = variant;
        State = state;

        _moveGenerator = moveGenerator;
        _moveExecutor = moveExecutor;
        _statusEvaluator = statusEvaluator;
    }

    public IEnumerable<Move> GenerateMoves(
        Square from)
    {
        return _moveGenerator.GenerateMoves(
            State,
            from);
    }

    public GameMoveRecord Execute(
        Move move)
    {
        var status = _statusEvaluator.Evaluate(State);

        if (status.IsTerminal)
        {
            throw new InvalidOperationException(
                "Cannot execute a move after the game has ended.");
        }

        return _moveExecutor.Execute(
            State,
            move);
    }

    public GameMoveRecord UndoLastMove()
    {
        return _moveExecutor.UndoLastMove(
            State);
    }
}