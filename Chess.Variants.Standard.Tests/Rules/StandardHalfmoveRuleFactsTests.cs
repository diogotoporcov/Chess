// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Variants.Standard.Games.History;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class StandardHalfmoveRuleFactsTests
{
    [Theory]
    [InlineData(0, false, false)]
    [InlineData(99, false, false)]
    [InlineData(100, true, false)]
    [InlineData(149, true, false)]
    [InlineData(150, true, true)]
    [InlineData(151, true, true)]
    public void ClockDeterminesThresholdFacts(
        int halfmoveClock,
        bool expectedFiftyMoveThreshold,
        bool expectedSeventyFiveMoveThreshold)
    {
        var facts = new StandardHalfmoveRuleFacts(halfmoveClock);

        Assert.Equal(halfmoveClock, facts.HalfmoveClock);
        Assert.Equal(
            expectedFiftyMoveThreshold,
            facts.IsFiftyMoveThresholdReached);
        Assert.Equal(
            expectedSeventyFiveMoveThreshold,
            facts.IsSeventyFiveMoveThresholdReached);
    }

    [Fact]
    public void NegativeClockIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StandardHalfmoveRuleFacts(-1));
    }
}
