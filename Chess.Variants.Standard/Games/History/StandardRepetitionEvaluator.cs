// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Movement;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardRepetitionEvaluator
{
    private readonly StandardPositionFactsEvaluator _positionFactsEvaluator;

    private readonly Func<GameState> _gameStateFactory;

    private readonly GameMoveExecutor _moveExecutor;

    public StandardRepetitionEvaluator(
        StandardPositionFactsEvaluator positionFactsEvaluator,
        Func<GameState> gameStateFactory,
        GameMoveExecutor moveExecutor)
    {
        ArgumentNullException.ThrowIfNull(positionFactsEvaluator);
        ArgumentNullException.ThrowIfNull(gameStateFactory);
        ArgumentNullException.ThrowIfNull(moveExecutor);

        _positionFactsEvaluator = positionFactsEvaluator;
        _gameStateFactory = gameStateFactory;
        _moveExecutor = moveExecutor;
    }

    public StandardRepetitionFacts Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var reconstructed = Reconstruct(gameState);
        var occurrences = reconstructed.PositionOccurrences[
            reconstructed.CurrentPositionKey];

        return new StandardRepetitionFacts(occurrences);
    }

    public bool WouldCreateThreefoldRepetition(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var reconstructed = Reconstruct(gameState);

        _moveExecutor.Execute(reconstructed.GameState, move);

        var resultingKey = _positionFactsEvaluator.CreatePositionKey(
            reconstructed.GameState);
        var occurrencesBeforeMove = reconstructed.PositionOccurrences
            .GetValueOrDefault(resultingKey);

        return occurrencesBeforeMove + 1 >= 3;
    }

    private ReconstructedHistory Reconstruct(
        GameState source)
    {
        var sourceKey = _positionFactsEvaluator.CreatePositionKey(source);
        var reconstructedState = _gameStateFactory() ??
                                 throw new InvalidOperationException(
                                     "The repetition game state factory " +
                                     "returned null.");
        var positionOccurrences = new Dictionary<StandardPositionKey, int>();
        var reconstructedKey = RecordPosition(
            reconstructedState,
            positionOccurrences);

        foreach (var record in source.History)
        {
            _moveExecutor.Execute(reconstructedState, record.Execution.Move);
            reconstructedKey = RecordPosition(
                reconstructedState,
                positionOccurrences);
        }

        if (reconstructedKey != sourceKey)
        {
            throw new InvalidOperationException(
                "The supplied game state cannot be reconstructed from the " +
                "repetition evaluator's initial game state factory.");
        }

        return new ReconstructedHistory(
            reconstructedState,
            reconstructedKey,
            positionOccurrences);
    }

    private StandardPositionKey RecordPosition(
        GameState gameState,
        Dictionary<StandardPositionKey, int> positionOccurrences)
    {
        var key = _positionFactsEvaluator.CreatePositionKey(gameState);

        positionOccurrences.TryGetValue(key, out var occurrences);
        positionOccurrences[key] = occurrences + 1;

        return key;
    }

    private sealed record ReconstructedHistory(
        GameState GameState,
        StandardPositionKey CurrentPositionKey,
        IReadOnlyDictionary<StandardPositionKey, int> PositionOccurrences);
}
