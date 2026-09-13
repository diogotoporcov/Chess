// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Movement;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardRepetitionEvaluator
{
    private readonly StandardPositionFactsEvaluator _positionFactsEvaluator;

    private readonly Func<Game> _gameFactory;

    public StandardRepetitionEvaluator(
        StandardPositionFactsEvaluator positionFactsEvaluator,
        Func<Game> gameFactory)
    {
        ArgumentNullException.ThrowIfNull(positionFactsEvaluator);
        ArgumentNullException.ThrowIfNull(gameFactory);

        _positionFactsEvaluator = positionFactsEvaluator;
        _gameFactory = gameFactory;
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

        reconstructed.Game.Execute(move);

        var resultingKey = _positionFactsEvaluator.CreatePositionKey(
            reconstructed.Game.State);
        var occurrencesBeforeMove = reconstructed.PositionOccurrences
            .GetValueOrDefault(resultingKey);

        return occurrencesBeforeMove + 1 >= 3;
    }

    private ReconstructedGame Reconstruct(
        GameState source)
    {
        var sourceKey = _positionFactsEvaluator.CreatePositionKey(source);
        var game = _gameFactory() ??
                   throw new InvalidOperationException(
                       "The repetition game factory returned null.");
        var positionOccurrences = new Dictionary<StandardPositionKey, int>();
        var reconstructedKey = RecordPosition(game.State, positionOccurrences);

        foreach (var record in source.History)
        {
            game.Execute(record.Execution.Move);
            reconstructedKey = RecordPosition(game.State, positionOccurrences);
        }

        if (reconstructedKey != sourceKey)
        {
            throw new InvalidOperationException(
                "The supplied game state cannot be reconstructed from the " +
                "repetition evaluator's initial game factory.");
        }

        return new ReconstructedGame(
            game,
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

    private sealed record ReconstructedGame(
        Game Game,
        StandardPositionKey CurrentPositionKey,
        IReadOnlyDictionary<StandardPositionKey, int> PositionOccurrences);
}
