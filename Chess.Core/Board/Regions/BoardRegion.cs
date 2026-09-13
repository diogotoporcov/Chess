// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Frozen;

namespace Chess.Core.Board.Regions;

public sealed class BoardRegion
{
    private readonly FrozenSet<Square> _squares;

    public BoardRegion(
        IEnumerable<Square> squares)
    {
        ArgumentNullException.ThrowIfNull(squares);

        _squares = squares.ToFrozenSet();

        if (_squares.Count == 0)
        {
            throw new ArgumentException(
                "Board region must contain at least one square.",
                nameof(squares));
        }
    }

    public bool Contains(
        Square square)
    {
        return _squares.Contains(square);
    }
}
