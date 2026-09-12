using Chess.Core.Board.Transitions;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Movement;

public sealed class MoveExecutionResolver :
    IMoveExecutionResolver
{
    private readonly IMoveExecutionResolver _basicResolver;

    public MoveExecutionResolver(
        IMoveExecutionResolver basicResolver)
    {
        ArgumentNullException.ThrowIfNull(basicResolver);

        _basicResolver = basicResolver;
    }

    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (move.OptionId is null)
        {
            return _basicResolver.Resolve(
                gameState,
                move);
        }

        if (!PromotionOptions.Contains(move.OptionId))
        {
            throw new InvalidOperationException(
                $"Unsupported move option '{move.OptionId}'.");
        }

        return ResolvePromotion(
            gameState,
            move);
    }

    private static MoveExecution ResolvePromotion(
        GameState gameState,
        Move move)
    {
        var boardState = gameState.BoardState;

        if (!boardState.TryGetPiece(
                move.From,
                out var pawn))
        {
            throw new InvalidOperationException(
                "Promotion move origin does not contain a piece.");
        }

        if (pawn.Definition.Id != PieceDefinitions.Pawn.Id)
        {
            throw new InvalidOperationException(
                "Only a pawn can be promoted.");
        }

        if (!gameState.MovementContext.IsInRegion(
                pawn.Side,
                BoardRegions.Promotion,
                move.To))
        {
            throw new InvalidOperationException(
                "Pawn promotion must end on a promotion square.");
        }

        boardState.TryGetPiece(
            move.To,
            out var destinationPiece);

        if (destinationPiece is not null &&
            destinationPiece.Side == pawn.Side)
        {
            throw new InvalidOperationException(
                "A pawn cannot promote onto a square occupied by a piece from the same side.");
        }

        var promotedDefinition =
            PromotionOptions.ResolvePieceDefinition(
                move.OptionId!);

        var promotedPiece =
            new Piece(
                pawn.Side,
                promotedDefinition);

        var transition =
            new BoardTransition(
                new BoardSquareChange(
                    move.From,
                    pawn,
                    null),
                new BoardSquareChange(
                    move.To,
                    destinationPiece,
                    promotedPiece));

        return new MoveExecution(
            move,
            transition);
    }
}