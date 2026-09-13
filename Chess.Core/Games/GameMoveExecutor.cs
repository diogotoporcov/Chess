// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class GameMoveExecutor
{
    private readonly IGameMoveGenerator _moveGenerator;

    private readonly IMoveExecutionResolver _executionResolver;

    public GameMoveExecutor(
        IGameMoveGenerator moveGenerator,
        IMoveExecutionResolver executionResolver)
    {
        ArgumentNullException.ThrowIfNull(moveGenerator);
        ArgumentNullException.ThrowIfNull(executionResolver);

        _moveGenerator = moveGenerator;
        _executionResolver = executionResolver;
    }

    public GameMoveRecord Execute(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(move.From, out var movingPiece))
        {
            throw new InvalidOperationException(
                "Move origin does not contain a piece.");
        }

        if (movingPiece.Side != gameState.CurrentSide)
        {
            throw new InvalidOperationException(
                "The piece does not belong to the side whose turn it is.");
        }

        var isAllowed = _moveGenerator
            .GenerateMoves(gameState, move.From)
            .Contains(move);

        if (!isAllowed)
        {
            throw new InvalidOperationException(
                "Move is not allowed in the current game state.");
        }

        var execution = _executionResolver.Resolve(gameState, move);

        if (execution.Move != move)
        {
            throw new InvalidOperationException(
                "Move execution resolver returned an execution for a different move.");
        }

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
