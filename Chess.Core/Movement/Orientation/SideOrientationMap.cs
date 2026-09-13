using System.Collections.Frozen;
using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Orientation;

public sealed class SideOrientationMap : IRelativeDirectionResolver
{
    private readonly FrozenDictionary<
        (Side Side, RelativeDirection RelativeDirection), Direction> _mappings;

    public SideOrientationMap(
        params ( Side Side, RelativeDirection RelativeDirection, Direction
            Direction)[] mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        if (mappings.Length == 0)
        {
            throw new ArgumentException(
                "At least one side orientation mapping is required.",
                nameof(mappings));
        }

        var dictionary =
            new Dictionary<(Side Side, RelativeDirection RelativeDirection),
                Direction>();

        foreach (var mapping in mappings)
        {
            ArgumentNullException.ThrowIfNull(mapping.Side);
            ArgumentNullException.ThrowIfNull(mapping.RelativeDirection);
            ArgumentNullException.ThrowIfNull(mapping.Direction);

            if (!dictionary.TryAdd(
                    (mapping.Side, mapping.RelativeDirection),
                    mapping.Direction))
            {
                throw new ArgumentException(
                    "A mapping already exists for this side and relative direction.",
                    nameof(mappings));
            }
        }

        _mappings = dictionary.ToFrozenDictionary();
    }

    public Direction Resolve(
        Side side,
        RelativeDirection relativeDirection)
    {
        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(relativeDirection);

        if (!_mappings.TryGetValue(
                (side, relativeDirection),
                out var direction))
        {
            throw new InvalidOperationException(
                $"No orientation mapping exists for side '{side}' " +
                $"and relative direction '{relativeDirection}'.");
        }

        return direction;
    }
}
