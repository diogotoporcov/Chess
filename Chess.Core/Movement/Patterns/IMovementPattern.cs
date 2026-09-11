using Chess.Core.Board;
using Chess.Core.Pieces;

namespace Chess.Core.Movement.Patterns;

public interface IMovementPattern
{
    IEnumerable<Move> GeneratePseudoLegalMoves(
        BoardState boardState,
        Square from,
        PieceColor movingColor);
}