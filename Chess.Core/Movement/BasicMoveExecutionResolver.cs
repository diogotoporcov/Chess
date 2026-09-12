using Chess.Core.Board.Transitions;
using Chess.Core.Games;

namespace Chess.Core.Movement;

public sealed class BasicMoveExecutionResolver :
    IMoveExecutionResolver
{
    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (move.OptionId is not null)
        {
            throw new InvalidOperationException(
                "Basic move execution cannot resolve a move with a specialized option.");
        }

        var boardState = gameState.BoardState;

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

        var transition =
            new BoardTransition(
                new BoardSquareChange(
                    move.From,
                    movingPiece,
                    null),
                new BoardSquareChange(
                    move.To,
                    destinationPiece,
                    movingPiece));

        return new MoveExecution(
            move,
            transition);
    }
}