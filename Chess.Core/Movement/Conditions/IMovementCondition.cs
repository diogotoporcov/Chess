using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Conditions;

public interface IMovementCondition
{
    bool IsSatisfied(
        MovementContext context,
        Square from,
        Side movingSide);
}