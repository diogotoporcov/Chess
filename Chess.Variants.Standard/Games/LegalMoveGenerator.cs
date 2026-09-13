// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Games.Rules;

namespace Chess.Variants.Standard.Games;

public sealed class LegalMoveGenerator : IGameMoveGenerator
{
    private readonly IGameMoveGenerator _pseudoLegalMoveGenerator;

    private readonly GameMoveSimulator _moveSimulator;

    private readonly CheckDetector _checkDetector;

    public LegalMoveGenerator(
        IGameMoveGenerator pseudoLegalMoveGenerator,
        GameMoveSimulator moveSimulator,
        CheckDetector checkDetector)
    {
        ArgumentNullException.ThrowIfNull(pseudoLegalMoveGenerator);
        ArgumentNullException.ThrowIfNull(moveSimulator);
        ArgumentNullException.ThrowIfNull(checkDetector);

        _pseudoLegalMoveGenerator = pseudoLegalMoveGenerator;

        _moveSimulator = moveSimulator;

        _checkDetector = checkDetector;
    }

    public IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(from, out var movingPiece))
        {
            yield break;
        }

        if (movingPiece.Side != gameState.CurrentSide)
        {
            yield break;
        }

        foreach (var move in _pseudoLegalMoveGenerator.GenerateMoves(
                     gameState,
                     from))
        {
            if (CapturesKing(gameState, move))
            {
                continue;
            }

            var leavesKingInCheck = _moveSimulator.Evaluate(
                gameState,
                move,
                (simulatedState, _) => _checkDetector.IsInCheck(
                    simulatedState,
                    movingPiece.Side));

            if (!leavesKingInCheck)
            {
                yield return move;
            }
        }
    }

    private static bool CapturesKing(
        GameState gameState,
        Move move)
    {
        return gameState.BoardState.TryGetPiece(move.To, out var targetPiece) &&
               KingRules.IsKing(targetPiece);
    }
}
