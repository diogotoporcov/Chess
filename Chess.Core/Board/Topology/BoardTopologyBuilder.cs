// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Board.Topology;

public sealed class BoardTopologyBuilder
{
    private readonly HashSet<Square> _squares = [];

    private readonly Dictionary<(Square From, Direction Direction), Square>
        _connections = [];

    public bool AddSquare(
        Square square)
    {
        return _squares.Add(square);
    }

    public void Connect(
        Square from,
        Direction direction,
        Square to)
    {
        EnsureSquareExists(from, nameof(from));
        EnsureSquareExists(to, nameof(to));

        if (!_connections.TryAdd((from, direction), to))
        {
            throw new InvalidOperationException(
                "A connection already exists for this square and direction.");
        }
    }

    public BoardTopology Build()
    {
        return new BoardTopology(_squares, _connections);
    }

    private void EnsureSquareExists(
        Square square,
        string parameterName)
    {
        if (!_squares.Contains(square))
        {
            throw new ArgumentException(
                "Square does not exist in this topology.",
                parameterName);
        }
    }
}
