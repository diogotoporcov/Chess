// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.ObjectModel;
using Chess.Core.Sides;

namespace Chess.Core.Games.Status;

public sealed class GameOutcome
{
    private readonly ReadOnlyCollection<Side> _winners;

    public GameTerminationId Termination { get; }

    public IReadOnlyList<Side> Winners => _winners;

    public GameOutcome(
        GameTerminationId termination,
        params Side[] winners)
    {
        ArgumentNullException.ThrowIfNull(termination);
        ArgumentNullException.ThrowIfNull(winners);

        foreach (var winner in winners)
        {
            ArgumentNullException.ThrowIfNull(winner);
        }

        if (winners
                .Distinct()
                .Count() !=
            winners.Length)
        {
            throw new ArgumentException(
                "A game outcome cannot contain the same winner more than once.",
                nameof(winners));
        }

        Termination = termination;
        _winners = Array.AsReadOnly([.. winners]);
    }
}
