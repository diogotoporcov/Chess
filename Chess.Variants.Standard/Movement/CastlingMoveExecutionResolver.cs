using Chess.Core.Board.Transitions;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Games.Rules;

namespace Chess.Variants.Standard.Movement;

public sealed class CastlingMoveExecutionResolver :
    IMoveExecutionResolver
{
    public bool CanResolve(
        Move move)
    {
        return
            move.OptionId == MoveOptions.CastleKingSide ||
            move.OptionId == MoveOptions.CastleQueenSide;
    }

    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!CanResolve(move))
        {
            throw new InvalidOperationException(
                "Move is not a castling move.");
        }

        if (!CastlingRules.TryValidateStructure(
                gameState,
                move,
                out var plan))
        {
            throw new InvalidOperationException(
                "Castling move is not valid in the current game state.");
        }

        var king =
            plan.King ??
            throw new InvalidOperationException(
                "Castling king could not be resolved.");

        var rook =
            plan.Rook ??
            throw new InvalidOperationException(
                "Castling rook could not be resolved.");

        var transition =
            new BoardTransition(
                new BoardSquareChange(
                    plan.KingFrom,
                    king,
                    null),
                new BoardSquareChange(
                    plan.KingTo,
                    null,
                    king),
                new BoardSquareChange(
                    plan.RookFrom,
                    rook,
                    null),
                new BoardSquareChange(
                    plan.RookTo,
                    null,
                    rook));

        return new MoveExecution(
            move,
            transition);
    }
}