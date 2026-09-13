// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board.Transitions;
using Chess.Core.Games;

namespace Chess.Core.Movement;

public sealed class BasicMoveExecutionResolver : IMoveExecutionResolver
{
    public bool CanResolve(
        Move move)
    {
        return move.OptionId is null;
    }

    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!CanResolve(move))
        {
            throw new InvalidOperationException(
                "Basic move execution cannot resolve a specialized move.");
        }

        var boardState = gameState.BoardState;

        if (!boardState.TryGetPiece(move.From, out var movingPiece))
        {
            throw new InvalidOperationException(
                "Move origin does not contain a piece.");
        }

        boardState.TryGetPiece(move.To, out var destinationPiece);

        if (destinationPiece is not null &&
            destinationPiece.Side == movingPiece.Side)
        {
            throw new InvalidOperationException(
                "A piece cannot capture another piece from the same side.");
        }

        var transition = new BoardTransition(
            new BoardSquareChange(move.From, movingPiece, null),
            new BoardSquareChange(move.To, destinationPiece, movingPiece));

        return new MoveExecution(move, transition);
    }
}
