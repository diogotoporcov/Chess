using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Orientation;

public interface IRelativeDirectionResolver
{
    Direction Resolve(
        Side side,
        RelativeDirection relativeDirection);
}
