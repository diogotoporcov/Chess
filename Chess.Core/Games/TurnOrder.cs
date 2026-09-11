using System.Collections.Frozen;
using System.Collections.ObjectModel;
using Chess.Core.Sides;

namespace Chess.Core.Games;

public sealed class TurnOrder
{
    private readonly ReadOnlyCollection<Side> _sides;

    private readonly FrozenDictionary<Side, int> _indexes;

    public IReadOnlyList<Side> Sides => _sides;

    public Side First => _sides[0];

    public TurnOrder(
        params Side[] sides)
    {
        ArgumentNullException.ThrowIfNull(sides);

        if (sides.Length == 0)
        {
            throw new ArgumentException(
                "Turn order must contain at least one side.",
                nameof(sides));
        }

        var indexes = new Dictionary<Side, int>();

        for (var index = 0; index < sides.Length; index++)
        {
            var side = sides[index];

            ArgumentNullException.ThrowIfNull(side);

            if (!indexes.TryAdd(
                    side,
                    index))
            {
                throw new ArgumentException(
                    "Turn order cannot contain the same side more than once.",
                    nameof(sides));
            }
        }

        _sides =
            Array.AsReadOnly(
            [
                .. sides
            ]);

        _indexes = indexes.ToFrozenDictionary();
    }

    public bool Contains(
        Side side)
    {
        ArgumentNullException.ThrowIfNull(side);

        return _indexes.ContainsKey(side);
    }

    public Side GetNext(
        Side side)
    {
        ArgumentNullException.ThrowIfNull(side);

        if (!_indexes.TryGetValue(
                side,
                out var index))
        {
            throw new ArgumentException(
                "Side is not part of this turn order.",
                nameof(side));
        }

        var nextIndex = (index + 1) % _sides.Count;

        return _sides[nextIndex];
    }
}