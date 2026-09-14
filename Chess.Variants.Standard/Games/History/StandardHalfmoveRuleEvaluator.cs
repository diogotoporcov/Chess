// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Movement;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardHalfmoveRuleEvaluator
{
    private readonly StandardPositionFactsEvaluator _positionFactsEvaluator;

    private readonly IGameMoveGenerator _legalMoveGenerator;

    private readonly IMoveExecutionResolver _executionResolver;

    public StandardHalfmoveRuleEvaluator(
        StandardPositionFactsEvaluator positionFactsEvaluator,
        IGameMoveGenerator legalMoveGenerator,
        IMoveExecutionResolver executionResolver)
    {
        ArgumentNullException.ThrowIfNull(positionFactsEvaluator);
        ArgumentNullException.ThrowIfNull(legalMoveGenerator);
        ArgumentNullException.ThrowIfNull(executionResolver);

        _positionFactsEvaluator = positionFactsEvaluator;
        _legalMoveGenerator = legalMoveGenerator;
        _executionResolver = executionResolver;
    }

    public StandardHalfmoveRuleFacts Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var halfmoveClock =
            _positionFactsEvaluator.EvaluateHalfmoveClock(gameState);

        return new StandardHalfmoveRuleFacts(halfmoveClock);
    }

    public bool WouldReachFiftyMoveThreshold(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var isLegal = _legalMoveGenerator
            .GenerateMoves(gameState, move.From)
            .Contains(move);

        if (!isLegal)
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

        var currentClock =
            _positionFactsEvaluator.EvaluateHalfmoveClock(gameState);
        var resultingClock = StandardHalfmoveRules.GetNextClock(
            currentClock,
            execution);

        return resultingClock >= StandardHalfmoveRules.FiftyMoveThreshold;
    }
}
