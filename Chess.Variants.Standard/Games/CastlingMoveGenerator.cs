// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Games.History;
using Chess.Variants.Standard.Games.Rules;

namespace Chess.Variants.Standard.Games;

public sealed class CastlingMoveGenerator : IGameMoveGenerator
{
    private readonly IGameMoveGenerator _innerMoveGenerator;

    private readonly GameMoveSimulator _moveSimulator;

    private readonly CheckDetector _checkDetector;

    private readonly CastlingRightsEvaluator _castlingRightsEvaluator;

    public CastlingMoveGenerator(
        IGameMoveGenerator innerMoveGenerator,
        GameMoveSimulator moveSimulator,
        CheckDetector checkDetector,
        CastlingRightsEvaluator castlingRightsEvaluator)
    {
        ArgumentNullException.ThrowIfNull(innerMoveGenerator);

        ArgumentNullException.ThrowIfNull(moveSimulator);

        ArgumentNullException.ThrowIfNull(checkDetector);
        ArgumentNullException.ThrowIfNull(castlingRightsEvaluator);

        _innerMoveGenerator = innerMoveGenerator;

        _moveSimulator = moveSimulator;

        _checkDetector = checkDetector;
        _castlingRightsEvaluator = castlingRightsEvaluator;
    }

    public IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        foreach (var move in _innerMoveGenerator.GenerateMoves(gameState, from))
        {
            yield return move;
        }

        if (!gameState.BoardState.TryGetPiece(from, out var king))
        {
            yield break;
        }

        if (_checkDetector.IsInCheck(gameState, king.Side))
        {
            yield break;
        }

        foreach (var move in CastlingRules.GenerateCandidates(
                     _castlingRightsEvaluator,
                     gameState,
                     from))
        {
            if (!CastlingRules.TryValidateStructure(
                    _castlingRightsEvaluator,
                    gameState,
                    move,
                    out var plan))
            {
                continue;
            }

            var throughMove = new Move(plan.KingFrom, plan.KingThrough);

            var crossesAttackedSquare = _moveSimulator.Evaluate(
                gameState,
                throughMove,
                (simulatedState, _) =>
                    _checkDetector.IsInCheck(simulatedState, king.Side));

            if (crossesAttackedSquare)
            {
                continue;
            }

            yield return move;
        }
    }
}
