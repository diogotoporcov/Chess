// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Variants.Standard.Games.History;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class StandardRepetitionFactsTests
{
    [Theory]
    [InlineData(1, false, false)]
    [InlineData(3, true, false)]
    [InlineData(5, true, true)]
    public void OccurrenceCountDeterminesRepetitionFacts(
        int occurrences,
        bool expectedThreefold,
        bool expectedFivefold)
    {
        var facts = new StandardRepetitionFacts(occurrences);

        Assert.Equal(occurrences, facts.CurrentPositionOccurrences);
        Assert.Equal(expectedThreefold, facts.IsThreefoldRepetition);
        Assert.Equal(expectedFivefold, facts.IsFivefoldRepetition);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void OccurrenceCountMustBePositive(
        int occurrences)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StandardRepetitionFacts(occurrences));
    }
}
