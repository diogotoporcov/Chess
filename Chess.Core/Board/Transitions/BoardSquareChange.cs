// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Pieces;

namespace Chess.Core.Board.Transitions;

public sealed record BoardSquareChange
{
    public Square Square { get; }

    public Piece? Before { get; }

    public Piece? After { get; }

    public BoardSquareChange(
        Square square,
        Piece? before,
        Piece? after)
    {
        if (before is null &&
            after is null)
        {
            throw new ArgumentException(
                "A board square change must contain a piece before or after the change.");
        }

        if (ReferenceEquals(before, after))
        {
            throw new ArgumentException(
                "The piece before and after the change cannot be the same instance.");
        }

        Square = square;
        Before = before;
        After = after;
    }
}
