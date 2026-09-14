// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Games;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;
using Chess.Core.Movement.Orientation;
using Chess.Core.Movement.Patterns;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Core.Tests.Games;

public sealed class GameStateFactoryTests
{
    [Fact]
    public void CreateBuildsConfiguredInitialStateAndMovementContext()
    {
        var topology = TestSupport.CreateGrid(2, 1);
        var turnOrder = new TurnOrder(TestSupport.White, TestSupport.Black);
        var from = TestSupport.At(1, 0, 1);
        var to = TestSupport.At(0, 0, 1);
        var forward = new RelativeDirection("test:forward");
        var regionId = new BoardRegionId("test:home");
        var placement = new InitialPiecePlacement(
            from,
            TestSupport.White,
            TestSupport.Token);
        var factory = new GameStateFactory(
            topology,
            turnOrder,
            new SideOrientationMap(
                (TestSupport.White, forward, CompassDirections.North),
                (TestSupport.Black, forward, CompassDirections.South)),
            new SideBoardRegionMap(
                (TestSupport.White, regionId, new BoardRegion([from]))),
            placement);

        var state = factory.Create();

        Assert.Same(topology, factory.Topology);
        Assert.Same(turnOrder, factory.TurnOrder);
        Assert.Equal([placement], factory.InitialPlacements);
        Assert.Same(topology, state.BoardState.Topology);
        Assert.Same(turnOrder, state.TurnOrder);
        Assert.Same(TestSupport.White, state.CurrentSide);
        Assert.Empty(state.History);
        Assert.Null(state.LastMove);
        Assert.True(state.BoardState.TryGetPiece(from, out var piece));
        Assert.Same(TestSupport.White, piece.Side);
        Assert.Same(TestSupport.Token, piece.Definition);
        Assert.True(
            state.MovementContext.IsInRegion(
                TestSupport.White,
                regionId,
                from));

        var pattern = new SlidingMovementPattern(forward, maxDistance: 1);

        Assert.Equal(
            [new Move(from, to)],
            pattern.GeneratePseudoLegalMoves(
                state.MovementContext,
                from,
                TestSupport.White));
    }

    [Fact]
    public void CreateReturnsIndependentStatesBoardsPiecesAndHistories()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var move = new Move(from, to);
        var factory = new GameStateFactory(
            topology,
            new TurnOrder(TestSupport.White, TestSupport.Black),
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            new InitialPiecePlacement(
                from,
                TestSupport.White,
                TestSupport.Token));

        var first = factory.Create();
        var second = factory.Create();

        Assert.NotSame(first, second);
        Assert.NotSame(first.BoardState, second.BoardState);
        Assert.True(first.BoardState.TryGetPiece(from, out var firstPiece));
        Assert.True(second.BoardState.TryGetPiece(from, out var secondPiece));
        Assert.NotSame(firstPiece, secondPiece);

        var executor = new GameMoveExecutor(
            new GameMoveResolver(
                new FixedMoveGenerator(move),
                new BasicMoveExecutionResolver()));

        executor.Execute(first, move);

        Assert.False(first.BoardState.IsOccupied(from));
        Assert.Same(firstPiece, PieceAt(first, to));
        Assert.Single(first.History);
        Assert.Same(TestSupport.Black, first.CurrentSide);
        Assert.Same(secondPiece, PieceAt(second, from));
        Assert.False(second.BoardState.IsOccupied(to));
        Assert.Empty(second.History);
        Assert.Null(second.LastMove);
        Assert.Same(TestSupport.White, second.CurrentSide);
    }

    [Fact]
    public void ExplicitInitialSideUsesCanonicalTurnOrderAcrossMoveAndUndo()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var turnOrder = new TurnOrder(TestSupport.White, TestSupport.Black);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var move = new Move(from, to);
        var factory = new GameStateFactory(
            topology,
            turnOrder,
            TestSupport.Black,
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            new InitialPiecePlacement(
                from,
                TestSupport.Black,
                TestSupport.Token));
        var state = factory.Create();
        var executor = new GameMoveExecutor(
            new GameMoveResolver(
                new FixedMoveGenerator(move),
                new BasicMoveExecutionResolver()));

        Assert.Equal([TestSupport.White, TestSupport.Black], turnOrder.Sides);
        Assert.Same(TestSupport.Black, factory.InitialSide);
        Assert.Same(TestSupport.Black, state.CurrentSide);

        executor.Execute(state, move);

        Assert.Same(TestSupport.White, state.CurrentSide);

        executor.UndoLastMove(state);

        Assert.Same(TestSupport.Black, state.CurrentSide);
    }

    [Fact]
    public void ConstructorRejectsInvalidPlacements()
    {
        var topology = TestSupport.CreateGrid(1, 1);
        var turnOrder = new TurnOrder(TestSupport.White, TestSupport.Black);
        var square = TestSupport.At(0, 0, 1);
        var placement = new InitialPiecePlacement(
            square,
            TestSupport.White,
            TestSupport.Token);

        Assert.Throws<ArgumentException>(() => new GameStateFactory(
            topology,
            turnOrder,
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            placement,
            placement));
        Assert.Throws<ArgumentException>(() => new GameStateFactory(
            topology,
            turnOrder,
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            new InitialPiecePlacement(
                new Square(10),
                TestSupport.White,
                TestSupport.Token)));
        Assert.Throws<ArgumentException>(() => new GameStateFactory(
            topology,
            turnOrder,
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            new InitialPiecePlacement(
                square,
                new Side("test:outsider"),
                TestSupport.Token)));
    }

    [Fact]
    public void ConstructorRejectsNullDependencies()
    {
        var topology = TestSupport.CreateGrid(1, 1);
        var turnOrder = new TurnOrder(TestSupport.White, TestSupport.Black);
        var orientationResolver = new ThrowingOrientationResolver();
        var regionResolver = new EmptyRegionResolver();

        Assert.Throws<ArgumentNullException>(() => new GameStateFactory(
            null!,
            turnOrder,
            orientationResolver,
            regionResolver));
        Assert.Throws<ArgumentNullException>(() => new GameStateFactory(
            topology,
            null!,
            orientationResolver,
            regionResolver));
        Assert.Throws<ArgumentNullException>(() => new GameStateFactory(
            topology,
            turnOrder,
            null!,
            regionResolver));
        Assert.Throws<ArgumentNullException>(() => new GameStateFactory(
            topology,
            turnOrder,
            orientationResolver,
            null!));
        Assert.Throws<ArgumentNullException>(() => new GameStateFactory(
            topology,
            turnOrder,
            orientationResolver,
            regionResolver,
            null!));
    }

    [Fact]
    public void ExplicitInitialSideMustBelongToTurnOrder()
    {
        var topology = TestSupport.CreateGrid(1, 1);
        var turnOrder = new TurnOrder(TestSupport.White, TestSupport.Black);

        Assert.Throws<ArgumentException>(() => new GameStateFactory(
            topology,
            turnOrder,
            new Side("test:outsider"),
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver()));

        var state = TestSupport.CreateState(topology, turnOrder);

        Assert.Throws<ArgumentException>(() => new GameState(
            state.MovementContext,
            turnOrder,
            new Side("test:outsider")));
    }

    [Fact]
    public void ConstructorRejectsNullPlacementElement()
    {
        var topology = TestSupport.CreateGrid(1, 1);
        var turnOrder = new TurnOrder(TestSupport.White, TestSupport.Black);
        var orientationResolver = new ThrowingOrientationResolver();
        var regionResolver = new EmptyRegionResolver();

        Assert.Throws<ArgumentException>(() => new GameStateFactory(
            topology,
            turnOrder,
            orientationResolver,
            regionResolver,
            [null!]));
    }

    private static Piece PieceAt(
        GameState gameState,
        Square square)
    {
        Assert.True(gameState.BoardState.TryGetPiece(square, out var piece));
        return piece;
    }
}
