// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class GameMoveExecutor
{
    private readonly GameMoveResolver _moveResolver;

    public GameMoveExecutor(
        GameMoveResolver moveResolver)
    {
        ArgumentNullException.ThrowIfNull(moveResolver);

        _moveResolver = moveResolver;
    }

    public GameMoveRecord Execute(
        GameState gameState,
        Move move)
    {
        var execution = _moveResolver.Resolve(gameState, move);

        gameState.BoardState.ApplyTransition(execution.Transition);

        try
        {
            return gameState.CommitMove(execution);
        }
        catch
        {
            gameState.BoardState.RevertTransition(execution.Transition);

            throw;
        }
    }

    public GameMoveRecord UndoLastMove(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var record = gameState.LastMove;

        if (record is null)
        {
            throw new InvalidOperationException("There is no move to undo.");
        }

        gameState.BoardState.RevertTransition(record.Execution.Transition);

        try
        {
            gameState.RollbackLastMove(record);
        }
        catch
        {
            gameState.BoardState.ApplyTransition(record.Execution.Transition);

            throw;
        }

        return record;
    }
}
