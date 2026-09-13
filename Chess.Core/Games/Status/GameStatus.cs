using System.Collections.ObjectModel;
using Chess.Core.Sides;

namespace Chess.Core.Games.Status;

public sealed class GameStatus
{
    private readonly ReadOnlyCollection<Side> _winners;

    public GameStatusId Id { get; }

    public bool IsTerminal { get; }

    public IReadOnlyList<Side> Winners => _winners;

    public GameStatus(
        GameStatusId id,
        bool isTerminal,
        params Side[] winners)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(winners);

        if (!isTerminal &&
            winners.Length > 0)
        {
            throw new ArgumentException(
                "A non-terminal game status cannot declare winners.",
                nameof(winners));
        }

        if (winners
                .Distinct()
                .Count() !=
            winners.Length)
        {
            throw new ArgumentException(
                "A game status cannot contain the same winner more than once.",
                nameof(winners));
        }

        Id = id;
        IsTerminal = isTerminal;

        _winners = Array.AsReadOnly([.. winners]);
    }
}
