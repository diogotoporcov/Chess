// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Pieces;

namespace Chess.Core.Board;

public readonly record struct PiecePosition
{
    public Square Square { get; }

    public Piece Piece { get; }

    public PiecePosition(
        Square square,
        Piece piece)
    {
        ArgumentNullException.ThrowIfNull(piece);

        Square = square;
        Piece = piece;
    }
}
