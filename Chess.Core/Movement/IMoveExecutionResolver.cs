// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;

namespace Chess.Core.Movement;

public interface IMoveExecutionResolver
{
    bool CanResolve(
        Move move);

    MoveExecution Resolve(
        GameState gameState,
        Move move);
}
