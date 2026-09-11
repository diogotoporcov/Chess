using Chess.Core.Board;
using Chess.Core.Board.Transitions;

namespace Chess.Core.Movement;

public sealed class MoveExecutor
{
    public MoveExecution Execute(
        BoardState boardState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(boardState);

        if (!boardState.TryGetPiece(
                move.From,
                out var movingPiece))
        {
            throw new InvalidOperationException(
                "Move origin does not contain a piece.");
        }

        boardState.TryGetPiece(
            move.To,
            out var destinationPiece);

        if (destinationPiece is not null &&
            destinationPiece.Side == movingPiece.Side)
        {
            throw new InvalidOperationException(
                "A piece cannot capture another piece from the same side.");
        }

        var transition = new BoardTransition(
            new BoardSquareChange(
                move.From,
                movingPiece,
                null),
            new BoardSquareChange(
                move.To,
                destinationPiece,
                movingPiece));

        boardState.ApplyTransition(
            transition);

        return new MoveExecution(
            move,
            transition);
    }

    public void Undo(
        BoardState boardState,
        MoveExecution execution)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        ArgumentNullException.ThrowIfNull(execution);

        boardState.RevertTransition(
            execution.Transition);
    }
}