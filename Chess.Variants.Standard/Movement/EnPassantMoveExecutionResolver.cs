using Chess.Core.Board.Transitions;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Games.Rules;

namespace Chess.Variants.Standard.Movement;

public sealed class EnPassantMoveExecutionResolver :
    IMoveExecutionResolver
{
    public bool CanResolve(
        Move move)
    {
        return move.OptionId == MoveOptions.EnPassant;
    }

    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!CanResolve(move))
        {
            throw new InvalidOperationException(
                "Move is not an en passant move.");
        }

        if (!EnPassantRules.TryGetCapturedPawn(
                gameState,
                move,
                out var capturedPawnSquare,
                out var capturedPawn))
        {
            throw new InvalidOperationException(
                "En passant move is not valid in the current game state.");
        }

        if (!gameState.BoardState.TryGetPiece(
                move.From,
                out var movingPawn))
        {
            throw new InvalidOperationException(
                "En passant move origin does not contain a pawn.");
        }

        var transition =
            new BoardTransition(
                new BoardSquareChange(
                    move.From,
                    movingPawn,
                    null),
                new BoardSquareChange(
                    capturedPawnSquare,
                    capturedPawn,
                    null),
                new BoardSquareChange(
                    move.To,
                    null,
                    movingPawn));

        return new MoveExecution(
            move,
            transition);
    }
}