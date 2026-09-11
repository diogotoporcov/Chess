using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public interface IMovementPattern
{
    IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from,
        Side movingSide);
}