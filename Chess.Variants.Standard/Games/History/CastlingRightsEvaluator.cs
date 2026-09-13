// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games.History;

public static class CastlingRightsEvaluator
{
    public static CastlingRights Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return new CastlingRights(
            HasRight(gameState, SideDefinitions.White, kingSide: true),
            HasRight(gameState, SideDefinitions.White, kingSide: false),
            HasRight(gameState, SideDefinitions.Black, kingSide: true),
            HasRight(gameState, SideDefinitions.Black, kingSide: false));
    }

    internal static bool HasRight(
        GameState gameState,
        Side side,
        bool kingSide)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(side);

        var homeRow = GetHomeRow(side);
        var kingSquare = BoardGeometry.SquareAt(homeRow, 4);
        var rookSquare = BoardGeometry.SquareAt(homeRow, kingSide ? 7 : 0);

        return HasUnmovedPiece(
                   gameState,
                   kingSquare,
                   side,
                   PieceDefinitions.King) &&
               HasUnmovedPiece(
                   gameState,
                   rookSquare,
                   side,
                   PieceDefinitions.Rook);
    }

    private static bool HasUnmovedPiece(
        GameState gameState,
        Square square,
        Side side,
        PieceDefinition definition)
    {
        return gameState.BoardState.TryGetPiece(square, out var piece) &&
               piece.Side == side &&
               piece.Definition.Id == definition.Id &&
               !HasMoved(gameState, piece);
    }

    private static bool HasMoved(
        GameState gameState,
        Piece piece)
    {
        foreach (var record in gameState.History)
        {
            foreach (var change in record.Execution.Transition.Changes)
            {
                if (ReferenceEquals(change.Before, piece) ||
                    ReferenceEquals(change.After, piece))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int GetHomeRow(
        Side side)
    {
        if (side == SideDefinitions.White)
        {
            return 7;
        }

        if (side == SideDefinitions.Black)
        {
            return 0;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }
}
