// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Variants.Standard.Movement;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardPositionFactsEvaluator
{
    private readonly IGameMoveGenerator _legalMoveGenerator;

    public StandardPositionFactsEvaluator(
        IGameMoveGenerator legalMoveGenerator)
    {
        ArgumentNullException.ThrowIfNull(legalMoveGenerator);

        _legalMoveGenerator = legalMoveGenerator;
    }

    public StandardPositionFacts Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return new StandardPositionFacts(
            CreatePositionKey(gameState),
            EvaluateHalfmoveClock(gameState));
    }

    public StandardPositionKey CreatePositionKey(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var castlingRights = CastlingRightsEvaluator.Evaluate(gameState);
        var effectiveEnPassantTarget =
            EvaluateEffectiveEnPassantTarget(gameState);

        return new StandardPositionKey(
            EvaluatePiecePlacements(gameState),
            gameState.CurrentSide.Id,
            castlingRights,
            effectiveEnPassantTarget);
    }

    public int EvaluateHalfmoveClock(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var halfmoveClock = 0;

        for (var index = gameState.History.Count - 1; index >= 0; index--)
        {
            var execution = gameState.History[index].Execution;

            if (StandardHalfmoveRules.ResetsClock(execution))
            {
                break;
            }

            halfmoveClock++;
        }

        return halfmoveClock;
    }

    private static IReadOnlyList<StandardPiecePlacement>
        EvaluatePiecePlacements(
            GameState gameState)
    {
        var placements = new List<StandardPiecePlacement>();

        foreach (var square in gameState.BoardState.Topology.Squares)
        {
            if (!gameState.BoardState.TryGetPiece(square, out var piece))
            {
                continue;
            }

            placements.Add(
                new StandardPiecePlacement(
                    square,
                    piece.Side.Id,
                    piece.Definition.Id.Value));
        }

        return placements;
    }

    private Square? EvaluateEffectiveEnPassantTarget(
        GameState gameState)
    {
        var targets = gameState
            .BoardState
            .GetPiecePositions(gameState.CurrentSide)
            .SelectMany(position => _legalMoveGenerator.GenerateMoves(
                gameState,
                position.Square))
            .Where(move => move.OptionId == MoveOptions.EnPassant)
            .Select(move => move.To)
            .Distinct()
            .ToArray();

        return targets.Length switch
        {
            0 => null,
            1 => targets[0],
            _ => throw new InvalidOperationException(
                "A standard position cannot have multiple en passant " +
                "target squares.")
        };
    }
}
