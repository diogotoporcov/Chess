// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Games.Rules;

internal static class InsufficientMatingMaterialDetector
{
    public static bool IsInsufficient(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var nonKingPieces = gameState
            .TurnOrder
            .Sides
            .SelectMany(gameState.BoardState.GetPiecePositions)
            .Where(position =>
                position.Piece.Definition.Id != PieceDefinitions.King.Id)
            .ToArray();

        return nonKingPieces.Length switch
        {
            0 => true,
            1 => IsBishopOrKnight(nonKingPieces[0]),
            _ => AreBishopsOnSameSquareColor(nonKingPieces)
        };
    }

    private static bool IsBishopOrKnight(
        PiecePosition position)
    {
        var definitionId = position.Piece.Definition.Id;

        return definitionId == PieceDefinitions.Bishop.Id ||
               definitionId == PieceDefinitions.Knight.Id;
    }

    private static bool AreBishopsOnSameSquareColor(
        PiecePosition[] positions)
    {
        if (positions.Any(position =>
                position.Piece.Definition.Id != PieceDefinitions.Bishop.Id))
        {
            return false;
        }

        var bishopSquareColor = GetSquareColor(positions[0].Square);

        return positions.All(position =>
            GetSquareColor(position.Square) == bishopSquareColor);
    }

    private static int GetSquareColor(
        Square square)
    {
        return (BoardLayout.GetRow(square) + BoardLayout.GetColumn(square)) & 1;
    }
}
