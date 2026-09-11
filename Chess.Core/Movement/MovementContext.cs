using Chess.Core.Board;
using Chess.Core.Movement.Orientation;
using Chess.Core.Sides;

namespace Chess.Core.Movement;

public sealed class MovementContext
{
    private readonly IRelativeDirectionResolver _relativeDirectionResolver;

    public BoardState BoardState { get; }

    public MovementContext(
        BoardState boardState,
        IRelativeDirectionResolver relativeDirectionResolver)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        ArgumentNullException.ThrowIfNull(relativeDirectionResolver);

        BoardState = boardState;
        _relativeDirectionResolver = relativeDirectionResolver;
    }

    internal Direction ResolveRelativeDirection(
        Side side,
        RelativeDirection relativeDirection)
    {
        return _relativeDirectionResolver.Resolve(
            side,
            relativeDirection);
    }
}