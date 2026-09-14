// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games.Status;

namespace Chess.Variants.Standard.Games;

public static class TerminationDefinitions
{
    public static GameTerminationId Checkmate { get; } = new("chess:checkmate");

    public static GameTerminationId Stalemate { get; } = new("chess:stalemate");

    public static GameTerminationId DeadPosition { get; } =
        new("chess:dead-position");

    public static GameTerminationId SeventyFiveMoveRule { get; } =
        new("chess:seventy-five-move-rule");

    public static GameTerminationId FivefoldRepetition { get; } =
        new("chess:fivefold-repetition");
}
