// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Movement;

public static class PromotionOptions
{
    public static readonly MoveOptionId Queen = new("chess:promotion:queen");
    public static readonly MoveOptionId Rook = new("chess:promotion:rook");
    public static readonly MoveOptionId Bishop = new("chess:promotion:bishop");
    public static readonly MoveOptionId Knight = new("chess:promotion:knight");

    public static IReadOnlyList<MoveOptionId> All { get; } =
        Array.AsReadOnly([Queen, Rook, Bishop, Knight]);

    internal static PieceDefinition ResolvePieceDefinition(
        MoveOptionId optionId)
    {
        ArgumentNullException.ThrowIfNull(optionId);

        if (optionId == Queen)
        {
            return PieceDefinitions.Queen;
        }

        if (optionId == Rook)
        {
            return PieceDefinitions.Rook;
        }

        if (optionId == Bishop)
        {
            return PieceDefinitions.Bishop;
        }

        if (optionId == Knight)
        {
            return PieceDefinitions.Knight;
        }

        throw new ArgumentException(
            $"Move option '{optionId}' is not a valid promotion option.",
            nameof(optionId));
    }

    internal static bool Contains(
        MoveOptionId optionId)
    {
        ArgumentNullException.ThrowIfNull(optionId);

        return All.Contains(optionId);
    }
}
