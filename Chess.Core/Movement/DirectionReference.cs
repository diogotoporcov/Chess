using Chess.Core.Board;
using Chess.Core.Movement.Orientation;
using Chess.Core.Sides;

namespace Chess.Core.Movement;

public abstract record DirectionReference
{
    internal abstract string SortKey { get; }

    internal abstract Direction Resolve(
        MovementContext context,
        Side side);

    public static implicit operator DirectionReference(
        Direction direction)
    {
        ArgumentNullException.ThrowIfNull(direction);

        return new AbsoluteDirectionReference(
            direction);
    }

    public static implicit operator DirectionReference(
        RelativeDirection direction)
    {
        ArgumentNullException.ThrowIfNull(direction);

        return new RelativeDirectionReference(
            direction);
    }

    private sealed record AbsoluteDirectionReference(
        Direction Direction)
        : DirectionReference
    {
        internal override string SortKey =>
            $"absolute:{Direction.Name}";

        internal override Direction Resolve(
            MovementContext context,
            Side side)
        {
            return Direction;
        }

        public override string ToString()
        {
            return Direction.ToString();
        }
    }

    private sealed record RelativeDirectionReference(
        RelativeDirection Direction)
        : DirectionReference
    {
        internal override string SortKey =>
            $"relative:{Direction.Name}";

        internal override Direction Resolve(
            MovementContext context,
            Side side)
        {
            return context.ResolveRelativeDirection(
                side,
                Direction);
        }

        public override string ToString()
        {
            return Direction.ToString();
        }
    }
}