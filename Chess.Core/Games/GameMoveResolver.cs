// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class GameMoveResolver
{
    private readonly IGameMoveGenerator _moveGenerator;

    private readonly IMoveExecutionResolver _executionResolver;

    public GameMoveResolver(
        IGameMoveGenerator moveGenerator,
        IMoveExecutionResolver executionResolver)
    {
        ArgumentNullException.ThrowIfNull(moveGenerator);
        ArgumentNullException.ThrowIfNull(executionResolver);

        _moveGenerator = moveGenerator;
        _executionResolver = executionResolver;
    }

    public MoveExecution Resolve(
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
                "Move execution resolver returned an execution " +
                "for a different move.");
        }

        return execution;
    }
}
