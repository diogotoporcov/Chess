// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Chess.Core.Board.Topology;
using Chess.Core.Board.Transitions;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Core.Board;

public sealed class BoardState
{
    private readonly Dictionary<Square, Piece> _pieces = [];

    private readonly Dictionary<Piece, Square> _pieceSquares =
        new(ReferenceEqualityComparer.Instance);

    public BoardTopology Topology { get; }

    internal BoardState(
        BoardTopology topology,
        IEnumerable<KeyValuePair<Square, Piece>> placements)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(placements);

        Topology = topology;

        foreach (var placement in placements)
        {
            var square = placement.Key;
            var piece = placement.Value;

            ArgumentNullException.ThrowIfNull(piece);

            EnsureSquareExists(square);

            if (!_pieces.TryAdd(square, piece))
            {
                throw new InvalidOperationException(
                    "Multiple pieces cannot occupy the same square.");
            }

            if (!_pieceSquares.TryAdd(piece, square))
            {
                throw new InvalidOperationException(
                    "The same piece cannot be placed on multiple squares.");
            }
        }
    }

    public bool IsOccupied(
        Square square)
    {
        EnsureSquareExists(square);

        return _pieces.ContainsKey(square);
    }

    public bool TryGetPiece(
        Square square,
        [NotNullWhen(true)] out Piece? piece)
    {
        EnsureSquareExists(square);

        return _pieces.TryGetValue(square, out piece);
    }

    public bool TryGetSquare(
        Piece piece,
        out Square square)
    {
        ArgumentNullException.ThrowIfNull(piece);

        return _pieceSquares.TryGetValue(piece, out square);
    }

    public IReadOnlyList<PiecePosition> GetPiecePositions(
        Side side)
    {
        ArgumentNullException.ThrowIfNull(side);

        return
        [
            .. _pieces
                .Where(entry => entry.Value.Side == side)
                .Select(entry => new PiecePosition(entry.Key, entry.Value))
        ];
    }

    internal void ApplyTransition(
        BoardTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);

        ApplyChanges(transition, reverse: false);
    }

    internal void RevertTransition(
        BoardTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);

        ApplyChanges(transition, reverse: true);
    }

    private void ApplyChanges(
        BoardTransition transition,
        bool reverse)
    {
        foreach (var change in transition.Changes)
        {
            EnsureSquareExists(change.Square);

            var expectedPiece = reverse ? change.After : change.Before;

            if (!MatchesCurrentPiece(change.Square, expectedPiece))
            {
                throw new InvalidOperationException(
                    "Board state does not match the expected transition state.");
            }
        }

        var affectedSquares = transition
            .Changes
            .Select(change => change.Square)
            .ToHashSet();

        var resultingPieces = transition
            .Changes
            .Select(change => reverse ? change.Before : change.After)
            .Where(piece => piece is not null)
            .Cast<Piece>()
            .ToArray();

        if (resultingPieces
            .GroupBy(piece => piece, ReferenceEqualityComparer.Instance)
            .Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException(
                "A board transition cannot place the same piece on multiple squares.");
        }

        foreach (var piece in resultingPieces)
        {
            if (_pieceSquares.TryGetValue(piece, out var currentSquare) &&
                !affectedSquares.Contains(currentSquare))
            {
                throw new InvalidOperationException(
                    "A board transition cannot place a piece that is already present on an unaffected square.");
            }
        }

        foreach (var change in transition.Changes)
        {
            if (_pieces.Remove(change.Square, out var currentPiece))
            {
                _pieceSquares.Remove(currentPiece);
            }
        }

        foreach (var change in transition.Changes)
        {
            var resultingPiece = reverse ? change.Before : change.After;

            if (resultingPiece is null)
            {
                continue;
            }

            _pieces.Add(change.Square, resultingPiece);

            _pieceSquares.Add(resultingPiece, change.Square);
        }
    }

    private bool MatchesCurrentPiece(
        Square square,
        Piece? expectedPiece)
    {
        if (!_pieces.TryGetValue(square, out var currentPiece))
        {
            return expectedPiece is null;
        }

        return ReferenceEquals(currentPiece, expectedPiece);
    }

    private void EnsureSquareExists(
        Square square)
    {
        if (!Topology.Contains(square))
        {
            throw new ArgumentException(
                "Square is not part of the board.",
                nameof(square));
        }
    }
}
