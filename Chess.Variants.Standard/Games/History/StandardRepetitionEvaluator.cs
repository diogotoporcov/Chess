// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Runtime.CompilerServices;
using Chess.Core.Games;
using Chess.Core.Movement;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardRepetitionEvaluator
{
    private const int MinimumHalfmovesForFivefoldRepetition = 16;

    private readonly StandardPositionFactsEvaluator _positionFactsEvaluator;

    private readonly Func<GameState> _gameStateFactory;

    private readonly GameMoveExecutor _moveExecutor;

    private readonly ConditionalWeakTable<GameState, RepetitionTracker>
        _trackers = new();

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

        var tracker = GetTracker(gameState);

        lock (tracker.SyncRoot)
        {
            SynchronizeSafely(tracker, gameState);

            var currentKey = tracker.PositionKeys[^1];
            var occurrences = tracker.PositionOccurrences[currentKey];

            return new StandardRepetitionFacts(occurrences);
        }
    }

    internal bool HasFivefoldRepetition(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        return gameState.History.Count >=
               MinimumHalfmovesForFivefoldRepetition &&
               Evaluate(gameState)
                   .IsFivefoldRepetition;
    }

    public bool WouldCreateThreefoldRepetition(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var tracker = GetTracker(gameState);

        lock (tracker.SyncRoot)
        {
            SynchronizeSafely(tracker, gameState);

            var candidateWasExecuted = false;

            try
            {
                _moveExecutor.Execute(tracker.ReconstructedState!, move);
                candidateWasExecuted = true;

                var resultingKey = _positionFactsEvaluator.CreatePositionKey(
                    tracker.ReconstructedState!);
                var occurrencesBeforeMove = tracker.PositionOccurrences
                    .GetValueOrDefault(resultingKey);

                return occurrencesBeforeMove + 1 >= 3;
            }
            finally
            {
                if (candidateWasExecuted)
                {
                    UndoCandidate(tracker);
                }
            }
        }
    }

    private RepetitionTracker GetTracker(
        GameState gameState)
    {
        return _trackers.GetValue(
            gameState,
            static _ => new RepetitionTracker());
    }

    private void SynchronizeSafely(
        RepetitionTracker tracker,
        GameState source)
    {
        try
        {
            Synchronize(tracker, source);
        }
        catch
        {
            tracker.Reset();
            throw;
        }
    }

    private void Synchronize(
        RepetitionTracker tracker,
        GameState source)
    {
        var wasInitialized = tracker.ReconstructedState is not null;

        if (tracker.ReconstructedState is null)
        {
            Initialize(tracker);
        }

        var commonPrefixLength = FindCommonPrefixLength(
            tracker.ProcessedSourceRecords,
            source.History);

        if (wasInitialized &&
            commonPrefixLength == tracker.ProcessedSourceRecords.Count &&
            commonPrefixLength == source.History.Count)
        {
            return;
        }

        RollBackTo(tracker, commonPrefixLength);

        for (var index = commonPrefixLength;
             index < source.History.Count;
             index++)
        {
            Append(tracker, source.History[index]);
        }

        var sourceKey = _positionFactsEvaluator.CreatePositionKey(source);
        var reconstructedKey = tracker.PositionKeys[^1];

        if (reconstructedKey != sourceKey)
        {
            throw new InvalidOperationException(
                "The supplied game state cannot be reconstructed from the " +
                "repetition evaluator's initial game state factory.");
        }
    }

    private void Initialize(
        RepetitionTracker tracker)
    {
        var reconstructedState = _gameStateFactory() ??
                                 throw new InvalidOperationException(
                                     "The repetition game state factory " +
                                     "returned null.");
        var initialKey = _positionFactsEvaluator.CreatePositionKey(
            reconstructedState);

        tracker.ReconstructedState = reconstructedState;
        tracker.PositionKeys.Add(initialKey);
        tracker.PositionOccurrences.Add(initialKey, 1);
    }

    private static int FindCommonPrefixLength(
        List<GameMoveRecord> processedSourceRecords,
        IReadOnlyList<GameMoveRecord> sourceHistory)
    {
        var commonLength = Math.Min(
            processedSourceRecords.Count,
            sourceHistory.Count);
        var index = 0;

        while (index < commonLength &&
               ReferenceEquals(
                   processedSourceRecords[index],
                   sourceHistory[index]))
        {
            index++;
        }

        return index;
    }

    private void UndoCandidate(
        RepetitionTracker tracker)
    {
        try
        {
            _moveExecutor.UndoLastMove(tracker.ReconstructedState!);
        }
        catch
        {
            tracker.Reset();
            throw;
        }
    }

    private void RollBackTo(
        RepetitionTracker tracker,
        int historyCount)
    {
        while (tracker.ProcessedSourceRecords.Count > historyCount)
        {
            var removedKey = tracker.PositionKeys[^1];
            var remainingOccurrences =
                tracker.PositionOccurrences[removedKey] - 1;

            if (remainingOccurrences == 0)
            {
                tracker.PositionOccurrences.Remove(removedKey);
            }
            else
            {
                tracker.PositionOccurrences[removedKey] = remainingOccurrences;
            }

            tracker.PositionKeys.RemoveAt(tracker.PositionKeys.Count - 1);
            tracker.ProcessedSourceRecords.RemoveAt(
                tracker.ProcessedSourceRecords.Count - 1);
            _moveExecutor.UndoLastMove(tracker.ReconstructedState!);
        }
    }

    private void Append(
        RepetitionTracker tracker,
        GameMoveRecord sourceRecord)
    {
        _moveExecutor.Execute(
            tracker.ReconstructedState!,
            sourceRecord.Execution.Move);

        var key = _positionFactsEvaluator.CreatePositionKey(
            tracker.ReconstructedState!);

        tracker.ProcessedSourceRecords.Add(sourceRecord);
        tracker.PositionKeys.Add(key);
        tracker.PositionOccurrences.TryGetValue(key, out var occurrences);
        tracker.PositionOccurrences[key] = occurrences + 1;
    }

    private sealed class RepetitionTracker
    {
        public object SyncRoot { get; } = new();

        public GameState? ReconstructedState { get; set; }

        public List<GameMoveRecord> ProcessedSourceRecords { get; } = [];

        public List<StandardPositionKey> PositionKeys { get; } = [];

        public Dictionary<StandardPositionKey, int> PositionOccurrences
        {
            get;
        } = [];

        public void Reset()
        {
            ReconstructedState = null;
            ProcessedSourceRecords.Clear();
            PositionKeys.Clear();
            PositionOccurrences.Clear();
        }
    }
}
