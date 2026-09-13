// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board.Transitions;

namespace Chess.Core.Movement;

public sealed record MoveExecution
{
    public Move Move { get; }

    public BoardTransition Transition { get; }

    public MoveExecution(
        Move move,
        BoardTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);

        Move = move;
        Transition = transition;
    }
}
