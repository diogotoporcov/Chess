// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Variants.Standard.Games.History;

public sealed record StandardRepetitionFacts
{
    public int CurrentPositionOccurrences { get; }

    public bool IsThreefoldRepetition => CurrentPositionOccurrences >= 3;

    public bool IsFivefoldRepetition => CurrentPositionOccurrences >= 5;

    public StandardRepetitionFacts(
        int currentPositionOccurrences)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            currentPositionOccurrences);

        CurrentPositionOccurrences = currentPositionOccurrences;
    }
}
