// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Games.Status;

public sealed class GameStatus
{
    public GameStatusId Id { get; }

    public GameOutcome? Outcome { get; }

    public bool IsTerminal => Outcome is not null;

    public GameStatus(
        GameStatusId id,
        GameOutcome? outcome = null)
    {
        ArgumentNullException.ThrowIfNull(id);

        Id = id;
        Outcome = outcome;
    }
}
