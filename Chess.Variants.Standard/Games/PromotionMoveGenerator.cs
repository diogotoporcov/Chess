using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Games;

public sealed class PromotionMoveGenerator : IGameMoveGenerator
{
    private readonly IGameMoveGenerator _innerMoveGenerator;

    public PromotionMoveGenerator(
        IGameMoveGenerator innerMoveGenerator)
    {
        ArgumentNullException.ThrowIfNull(innerMoveGenerator);

        _innerMoveGenerator = innerMoveGenerator;
    }

    public IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(from, out var movingPiece))
        {
            yield break;
        }

        foreach (var move in _innerMoveGenerator.GenerateMoves(gameState, from))
        {
            if (movingPiece.Definition.Id != PieceDefinitions.Pawn.Id)
            {
                yield return move;
                continue;
            }

            var reachesPromotionRegion = gameState.MovementContext.IsInRegion(
                movingPiece.Side,
                BoardRegions.Promotion,
                move.To);

            if (!reachesPromotionRegion)
            {
                yield return move;
                continue;
            }

            foreach (var option in PromotionOptions.All)
            {
                yield return new Move(move.From, move.To, option);
            }
        }
    }
}
