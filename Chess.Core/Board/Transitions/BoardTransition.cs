// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Board.Transitions;

public sealed class BoardTransition
{
    public IReadOnlyList<BoardSquareChange> Changes { get; }

    public BoardTransition(
        params BoardSquareChange[] changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        if (changes.Length == 0)
        {
            throw new ArgumentException(
                "A board transition must contain at least one square change.",
                nameof(changes));
        }

        if (changes
            .GroupBy(change => change.Square)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "A board transition cannot contain multiple changes for the same square.",
                nameof(changes));
        }

        Changes = Array.AsReadOnly([.. changes]);
    }
}
