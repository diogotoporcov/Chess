// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;

namespace Chess.Variants.Standard.Movement;

public static class MoveOptions
{
    public static MoveOptionId EnPassant { get; } = new("chess:en-passant");

    public static MoveOptionId CastleKingSide { get; } =
        new("chess:castle:kingside");

    public static MoveOptionId CastleQueenSide { get; } =
        new("chess:castle:queenside");
}
