// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;

namespace Chess.Variants.Standard.Games.History;

public sealed record StandardPositionFacts
{
    public StandardPositionKey PositionKey { get; }

    public int HalfmoveClock { get; }

    public int FullmoveNumber { get; }

    public Square? EnPassantTarget { get; }

    public CastlingRights CastlingRights => PositionKey.CastlingRights;

    public Square? EffectiveEnPassantTarget =>
        PositionKey.EffectiveEnPassantTarget;

    public StandardPositionFacts(
        StandardPositionKey positionKey,
        Square? enPassantTarget,
        int halfmoveClock,
        int fullmoveNumber)
    {
        ArgumentNullException.ThrowIfNull(positionKey);

        if (halfmoveClock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(halfmoveClock));
        }

        if (fullmoveNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(fullmoveNumber));
        }

        PositionKey = positionKey;
        EnPassantTarget = enPassantTarget;
        HalfmoveClock = halfmoveClock;
        FullmoveNumber = fullmoveNumber;
    }
}
