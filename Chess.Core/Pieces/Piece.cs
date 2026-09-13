// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Movement;
using Chess.Core.Sides;

namespace Chess.Core.Pieces;

public sealed class Piece
{
    public Side Side { get; }

    public PieceDefinition Definition { get; }

    public Piece(
        Side side,
        PieceDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(definition);

        Side = side;
        Definition = definition;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(context);

        EnsurePlacedAt(context.BoardState, from);

        var generatedMoves = new HashSet<Move>();

        foreach (var pattern in Definition.MovementPatterns)
        {
            foreach (var move in pattern.GeneratePseudoLegalMoves(
                         context,
                         from,
                         Side))
            {
                if (generatedMoves.Add(move))
                {
                    yield return move;
                }
            }
        }
    }

    public IEnumerable<Square> GenerateAttackedSquares(
        MovementContext context,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(context);

        EnsurePlacedAt(context.BoardState, from);

        var attackedSquares = new HashSet<Square>();

        foreach (var pattern in Definition.MovementPatterns)
        {
            foreach (var square in pattern.GenerateAttackedSquares(
                         context,
                         from,
                         Side))
            {
                if (attackedSquares.Add(square))
                {
                    yield return square;
                }
            }
        }
    }

    private void EnsurePlacedAt(
        BoardState boardState,
        Square square)
    {
        if (!boardState.TryGetPiece(square, out var occupyingPiece) ||
            !ReferenceEquals(occupyingPiece, this))
        {
            throw new InvalidOperationException(
                "This piece is not placed on the specified square.");
        }
    }
}
