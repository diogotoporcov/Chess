// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Movement;

namespace Chess.Core.Games;

public sealed class PseudoLegalGameMoveGenerator : IGameMoveGenerator
{
    public IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(from, out var piece))
        {
            yield break;
        }

        if (piece.Side != gameState.CurrentSide)
        {
            yield break;
        }

        foreach (var move in piece.GeneratePseudoLegalMoves(
                     gameState.MovementContext,
                     from))
        {
            yield return move;
        }
    }
}
