// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Transitions;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Core.Pieces;

namespace Chess.Core.Tests.Board;

public sealed class BoardStateAndTransitionTests
{
    [Fact]
    public void Builder_MaintainsSquareAndPieceIndexes()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var square = TestSupport.At(0, 0, 2);
        var piece = TestSupport.Piece();
        var board = new BoardStateBuilder(topology)
            .PlacePiece(square, piece)
            .Build();

        Assert.True(board.IsOccupied(square));
        Assert.True(board.TryGetPiece(square, out var found));
        Assert.Same(piece, found);
        Assert.True(board.TryGetSquare(piece, out var foundSquare));
        Assert.Equal(square, foundSquare);
        Assert.Single(board.GetPiecePositions(TestSupport.White));
        Assert.Empty(board.GetPiecePositions(TestSupport.Black));
    }

    [Fact]
    public void
        Builder_RejectsUnknownSquaresDuplicateOccupancyAndDuplicatePiece()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var first = TestSupport.At(0, 0, 2);
        var second = TestSupport.At(0, 1, 2);
        var piece = TestSupport.Piece();
        var builder = new BoardStateBuilder(topology);

        Assert.Throws<ArgumentException>(() =>
            builder.PlacePiece(new Square(50), piece));

        builder.PlacePiece(first, piece);

        Assert.Throws<InvalidOperationException>(() =>
            builder.PlacePiece(first, TestSupport.Piece()));
        Assert.Throws<InvalidOperationException>(() =>
            builder.PlacePiece(second, piece));
    }

    [Fact]
    public void BoardQueries_RejectSquaresOutsideTheTopology()
    {
        var board = new BoardStateBuilder(TestSupport.CreateGrid(1, 1)).Build();
        var outside = new Square(10);

        Assert.Throws<ArgumentException>(() => board.IsOccupied(outside));
        Assert.Throws<ArgumentException>(() =>
            board.TryGetPiece(outside, out _));
    }

    [Fact]
    public void BoardSquareChange_RejectsNoOpChanges()
    {
        var square = new Square(1);
        var piece = TestSupport.Piece();

        Assert.Throws<ArgumentException>(() =>
            new BoardSquareChange(square, null, null));
        Assert.Throws<ArgumentException>(() =>
            new BoardSquareChange(square, piece, piece));
    }

    [Fact]
    public void BoardTransition_RequiresUniqueChangedSquares()
    {
        var square = new Square(1);
        var piece = TestSupport.Piece();
        var change = new BoardSquareChange(square, piece, null);

        Assert.Throws<ArgumentException>(() => new BoardTransition());
        Assert.Throws<ArgumentException>(() =>
            new BoardTransition(change, change));
    }

    [Fact]
    public void ExecuteAndUndo_CaptureRestoresExactBoardIndexesAndHistory()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var attacker = TestSupport.Piece();
        var captured = TestSupport.Piece(TestSupport.Black);
        var move = new Move(from, to);
        var state = TestSupport.CreateState(
            topology,
            null,
            (from, attacker),
            (to, captured));
        var before = GameStateSnapshot.Capture(state);
        var executor = new GameMoveExecutor(
            new FixedMoveGenerator(move),
            new BasicMoveExecutionResolver());

        var record = executor.Execute(state, move);

        Assert.Equal(1, record.PlyNumber);
        Assert.Same(TestSupport.White, record.Side);
        Assert.Same(attacker, AssertPiece(state, to));
        Assert.False(state.BoardState.IsOccupied(from));
        Assert.False(state.BoardState.TryGetSquare(captured, out _));
        Assert.Same(TestSupport.Black, state.CurrentSide);
        Assert.Same(record, state.LastMove);

        Assert.Same(record, executor.UndoLastMove(state));
        before.AssertMatches(state);
    }

    [Fact]
    public void ExecuteAndUndo_MultiSquareTransitionIsReversible()
    {
        var topology = TestSupport.CreateGrid(1, 4);
        var squares = Enumerable
            .Range(0, 4)
            .Select(column => TestSupport.At(0, column, 4))
            .ToArray();
        var first = TestSupport.Piece();
        var second = TestSupport.Piece();
        var move = new Move(squares[0], squares[1]);
        var transition = new BoardTransition(
            new BoardSquareChange(squares[0], first, null),
            new BoardSquareChange(squares[1], null, first),
            new BoardSquareChange(squares[3], second, null),
            new BoardSquareChange(squares[2], null, second));
        var state = TestSupport.CreateState(
            topology,
            null,
            (squares[0], first),
            (squares[3], second));
        var before = GameStateSnapshot.Capture(state);
        var executor = new GameMoveExecutor(
            new FixedMoveGenerator(move),
            new DelegateResolver((_, _) =>
                new MoveExecution(move, transition)));

        executor.Execute(state, move);

        Assert.Same(first, AssertPiece(state, squares[1]));
        Assert.Same(second, AssertPiece(state, squares[2]));

        executor.UndoLastMove(state);
        before.AssertMatches(state);
    }

    [Fact]
    public void Execute_RejectsAStaleTransitionWithoutChangingState()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var actual = TestSupport.Piece();
        var stale = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(topology, null, (from, actual));
        var before = GameStateSnapshot.Capture(state);
        var transition = new BoardTransition(
            new BoardSquareChange(from, stale, null),
            new BoardSquareChange(to, null, stale));
        var executor = new GameMoveExecutor(
            new FixedMoveGenerator(move),
            new DelegateResolver((_, _) =>
                new MoveExecution(move, transition)));

        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, move));
        before.AssertMatches(state);
    }

    [Fact]
    public void Execute_RejectsPlacingOnePieceOnMultipleSquares()
    {
        var topology = TestSupport.CreateGrid(1, 3);
        var from = TestSupport.At(0, 0, 3);
        var middle = TestSupport.At(0, 1, 3);
        var to = TestSupport.At(0, 2, 3);
        var piece = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(topology, null, (from, piece));
        var transition = new BoardTransition(
            new BoardSquareChange(from, piece, null),
            new BoardSquareChange(middle, null, piece),
            new BoardSquareChange(to, null, piece));
        var executor = new GameMoveExecutor(
            new FixedMoveGenerator(move),
            new DelegateResolver((_, _) =>
                new MoveExecution(move, transition)));

        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, move));
    }

    [Fact]
    public void Execute_RejectsPlacingPieceAlreadyOnUnaffectedSquare()
    {
        var topology = TestSupport.CreateGrid(1, 3);
        var from = TestSupport.At(0, 0, 3);
        var to = TestSupport.At(0, 1, 3);
        var unaffected = TestSupport.At(0, 2, 3);
        var moving = TestSupport.Piece();
        var alreadyPlaced = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(
            topology,
            null,
            (from, moving),
            (unaffected, alreadyPlaced));
        var transition = new BoardTransition(
            new BoardSquareChange(from, moving, null),
            new BoardSquareChange(to, null, alreadyPlaced));
        var executor = new GameMoveExecutor(
            new FixedMoveGenerator(move),
            new DelegateResolver((_, _) =>
                new MoveExecution(move, transition)));

        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, move));
        Assert.Same(alreadyPlaced, AssertPiece(state, unaffected));
    }

    private static Piece AssertPiece(
        GameState state,
        Square square)
    {
        Assert.True(state.BoardState.TryGetPiece(square, out var piece));
        return piece;
    }
}
