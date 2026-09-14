// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Status;
using Chess.Core.Sides;
using Chess.Variants.Standard.Games.History;

namespace Chess.Variants.Standard.Games.Rules;

public sealed class StatusEvaluator : IGameStatusEvaluator
{
    private readonly IGameMoveGenerator _legalMoveGenerator;

    private readonly CheckDetector _checkDetector;

    private readonly StandardRepetitionEvaluator _repetitionEvaluator;

    private readonly StandardHalfmoveRuleEvaluator _halfmoveRuleEvaluator;

    public StatusEvaluator(
        IGameMoveGenerator legalMoveGenerator,
        CheckDetector checkDetector,
        StandardRepetitionEvaluator repetitionEvaluator,
        StandardHalfmoveRuleEvaluator halfmoveRuleEvaluator)
    {
        ArgumentNullException.ThrowIfNull(legalMoveGenerator);
        ArgumentNullException.ThrowIfNull(checkDetector);
        ArgumentNullException.ThrowIfNull(repetitionEvaluator);
        ArgumentNullException.ThrowIfNull(halfmoveRuleEvaluator);

        _legalMoveGenerator = legalMoveGenerator;
        _checkDetector = checkDetector;
        _repetitionEvaluator = repetitionEvaluator;
        _halfmoveRuleEvaluator = halfmoveRuleEvaluator;
    }

    public GameStatus Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var currentSide = gameState.CurrentSide;

        var isInCheck = _checkDetector.IsInCheck(gameState, currentSide);

        var hasLegalMove = HasLegalMove(gameState, currentSide);

        if (!hasLegalMove)
        {
            if (!isInCheck)
            {
                return new GameStatus(
                    StatusDefinitions.Stalemate,
                    new GameOutcome(TerminationDefinitions.Stalemate));
            }

            var winner = FindWinningSide(gameState, currentSide);

            return new GameStatus(
                StatusDefinitions.Checkmate,
                new GameOutcome(TerminationDefinitions.Checkmate, winner));
        }

        var statusId = isInCheck
            ? StatusDefinitions.Check
            : StatusDefinitions.Active;

        if (InsufficientMatingMaterialDetector.IsInsufficient(gameState))
        {
            return new GameStatus(
                statusId,
                new GameOutcome(TerminationDefinitions.DeadPosition));
        }

        var halfmoveFacts = _halfmoveRuleEvaluator.Evaluate(gameState);

        if (halfmoveFacts.IsSeventyFiveMoveThresholdReached)
        {
            return new GameStatus(
                statusId,
                new GameOutcome(TerminationDefinitions.SeventyFiveMoveRule));
        }

        if (_repetitionEvaluator.HasFivefoldRepetition(gameState))
        {
            return new GameStatus(
                statusId,
                new GameOutcome(TerminationDefinitions.FivefoldRepetition));
        }

        return new GameStatus(statusId);
    }

    private bool HasLegalMove(
        GameState gameState,
        Side side)
    {
        foreach (var position in gameState.BoardState.GetPiecePositions(side))
        {
            if (_legalMoveGenerator
                .GenerateMoves(gameState, position.Square)
                .Any())
            {
                return true;
            }
        }

        return false;
    }

    private static Side FindWinningSide(
        GameState gameState,
        Side losingSide)
    {
        var opponents = gameState
            .TurnOrder
            .Sides
            .Where(side => side != losingSide)
            .ToArray();

        if (opponents.Length != 1)
        {
            throw new InvalidOperationException(
                "Standard chess requires exactly one opposing side.");
        }

        return opponents[0];
    }
}
