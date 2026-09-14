// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Variants.Standard.Games.History;

public sealed record StandardHalfmoveRuleFacts
{
    public int HalfmoveClock { get; }

    public bool IsFiftyMoveThresholdReached =>
        HalfmoveClock >= StandardHalfmoveRules.FiftyMoveThreshold;

    public bool IsSeventyFiveMoveThresholdReached =>
        HalfmoveClock >= StandardHalfmoveRules.SeventyFiveMoveThreshold;

    public StandardHalfmoveRuleFacts(
        int halfmoveClock)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(halfmoveClock);

        HalfmoveClock = halfmoveClock;
    }
}
