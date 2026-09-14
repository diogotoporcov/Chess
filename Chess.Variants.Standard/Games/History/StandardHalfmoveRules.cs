// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Games.History;

internal static class StandardHalfmoveRules
{
    public const int FiftyMoveThreshold = 100;

    public const int SeventyFiveMoveThreshold = 150;

    public static bool ResetsClock(
        MoveExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution);

        return IsPawnMove(execution) || IsCapture(execution);
    }

    public static int GetNextClock(
        int currentClock,
        MoveExecution execution)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(currentClock);

        return ResetsClock(execution) ? 0 : checked(currentClock + 1);
    }

    private static bool IsPawnMove(
        MoveExecution execution)
    {
        var originChange =
            execution.Transition.Changes.SingleOrDefault(change =>
                change.Square == execution.Move.From);

        if (originChange?.Before is null)
        {
            throw new InvalidOperationException(
                "Move execution does not identify the moving piece " +
                "at its origin.");
        }

        return originChange.Before.Definition.Id == PieceDefinitions.Pawn.Id;
    }

    private static bool IsCapture(
        MoveExecution execution)
    {
        foreach (var change in execution.Transition.Changes)
        {
            if (change.Square == execution.Move.From ||
                change.Before is null)
            {
                continue;
            }

            var pieceRemainsOnBoard =
                execution.Transition.Changes.Any(destination =>
                    ReferenceEquals(destination.After, change.Before));

            if (!pieceRemainsOnBoard)
            {
                return true;
            }
        }

        return false;
    }
}
