// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.ObjectModel;
using Chess.Core.Board;
using Chess.Core.Movement;
using Chess.Core.Sides;

namespace Chess.Core.Games;

public sealed class GameState
{
    private readonly List<GameMoveRecord> _history = [];

    private readonly ReadOnlyCollection<GameMoveRecord> _historyView;

    public MovementContext MovementContext { get; }

    public BoardState BoardState => MovementContext.BoardState;

    public TurnOrder TurnOrder { get; }

    public Side CurrentSide { get; private set; }

    public IReadOnlyList<GameMoveRecord> History => _historyView;

    public GameMoveRecord? LastMove =>
        _history.Count == 0 ? null : _history[^1];

    public GameState(
        MovementContext movementContext,
        TurnOrder turnOrder)
    {
        ArgumentNullException.ThrowIfNull(movementContext);

        ArgumentNullException.ThrowIfNull(turnOrder);

        MovementContext = movementContext;
        TurnOrder = turnOrder;

        CurrentSide = turnOrder.First;

        _historyView = _history.AsReadOnly();
    }

    internal GameMoveRecord CommitMove(
        MoveExecution execution)
    {
        ArgumentNullException.ThrowIfNull(execution);

        var movingSide = CurrentSide;

        var nextSide = TurnOrder.GetNext(movingSide);

        var record = new GameMoveRecord(
            _history.Count + 1,
            movingSide,
            execution);

        _history.Add(record);

        CurrentSide = nextSide;

        return record;
    }

    internal void RollbackLastMove(
        GameMoveRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (_history.Count == 0 ||
            !ReferenceEquals(_history[^1], record))
        {
            throw new InvalidOperationException(
                "The specified move is not the last move in the game history.");
        }

        _history.RemoveAt(_history.Count - 1);

        CurrentSide = record.Side;
    }
}
