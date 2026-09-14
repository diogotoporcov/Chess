// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Movement;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardHalfmoveRuleEvaluator
{
    private readonly StandardPositionFactsEvaluator _positionFactsEvaluator;

    private readonly GameMoveResolver _moveResolver;

    public StandardHalfmoveRuleEvaluator(
        StandardPositionFactsEvaluator positionFactsEvaluator,
        GameMoveResolver moveResolver)
    {
        ArgumentNullException.ThrowIfNull(positionFactsEvaluator);
        ArgumentNullException.ThrowIfNull(moveResolver);

        _positionFactsEvaluator = positionFactsEvaluator;
        _moveResolver = moveResolver;
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
        var execution = _moveResolver.Resolve(gameState, move);

        var currentClock =
            _positionFactsEvaluator.EvaluateHalfmoveClock(gameState);
        var resultingClock = StandardHalfmoveRules.GetNextClock(
            currentClock,
            execution);

        return resultingClock >= StandardHalfmoveRules.FiftyMoveThreshold;
    }
}
