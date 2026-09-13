// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Variants.Standard.Games.History;

public readonly record struct CastlingRights
{
    public bool WhiteKingSide { get; }

    public bool WhiteQueenSide { get; }

    public bool BlackKingSide { get; }

    public bool BlackQueenSide { get; }

    public CastlingRights(
        bool whiteKingSide,
        bool whiteQueenSide,
        bool blackKingSide,
        bool blackQueenSide)
    {
        WhiteKingSide = whiteKingSide;
        WhiteQueenSide = whiteQueenSide;
        BlackKingSide = blackKingSide;
        BlackQueenSide = blackQueenSide;
    }
}
