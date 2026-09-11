using Chess.Core.Board;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class Game
{
    private readonly IGameMoveGenerator _moveGenerator;

    private readonly GameMoveExecutor _moveExecutor;

    public GameVariantDefinition Variant { get; }

    public GameState State { get; }

    public BoardState BoardState =>
        State.BoardState;

    internal Game(
        GameVariantDefinition variant,
        GameState state,
        IGameMoveGenerator moveGenerator,
        GameMoveExecutor moveExecutor)
    {
        ArgumentNullException.ThrowIfNull(variant);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(moveGenerator);
        ArgumentNullException.ThrowIfNull(moveExecutor);

        Variant = variant;
        State = state;

        _moveGenerator = moveGenerator;
        _moveExecutor = moveExecutor;
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