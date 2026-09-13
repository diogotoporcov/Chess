// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Frozen;

namespace Chess.Core.Board.Topology;

public sealed class BoardTopology
{
    private readonly FrozenSet<Square> _squares;

    private readonly FrozenDictionary<
        (Square From, Direction Direction), Square> _connections;

    public IReadOnlySet<Square> Squares => _squares;

    internal BoardTopology(
        IEnumerable<Square> squares,
        IEnumerable<KeyValuePair<(Square From, Direction Direction), Square>>
            connections)
    {
        _squares = squares.ToFrozenSet();
        _connections = connections.ToFrozenDictionary();
    }

    public bool Contains(
        Square square)
    {
        return _squares.Contains(square);
    }

    public bool TryGetNext(
        Square from,
        Direction direction,
        out Square next)
    {
        EnsureSquareExists(from);

        return _connections.TryGetValue((from, direction), out next);
    }

    private void EnsureSquareExists(
        Square square)
    {
        if (!_squares.Contains(square))
        {
            throw new ArgumentException(
                "Square does not exist in this topology.",
                nameof(square));
        }
    }
}
