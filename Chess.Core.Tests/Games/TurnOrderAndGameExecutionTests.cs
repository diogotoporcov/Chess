// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Topology;
using Chess.Core.Board.Transitions;
using Chess.Core.Games;
using Chess.Core.Games.Status;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;
using Chess.Core.Movement.Patterns;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Core.Tests.Games;

public sealed class TurnOrderAndGameExecutionTests
{
    [Fact]
    public void TurnOrder_UsesFirstSideAndWrapsAround()
    {
        var third = new Side("test:third");
        var order = new TurnOrder(TestSupport.White, TestSupport.Black, third);

        Assert.Same(TestSupport.White, order.First);
        Assert.Same(TestSupport.Black, order.GetNext(TestSupport.White));
        Assert.Same(third, order.GetNext(TestSupport.Black));
        Assert.Same(TestSupport.White, order.GetNext(third));
        Assert.True(order.Contains(TestSupport.Black));
    }

    [Fact]
    public void TurnOrder_RejectsEmptyDuplicateAndUnknownSides()
    {
        Assert.Throws<ArgumentException>(() => new TurnOrder());
        Assert.Throws<ArgumentException>(() =>
            new TurnOrder(TestSupport.White, TestSupport.White));

        var order = new TurnOrder(TestSupport.White, TestSupport.Black);

        Assert.Throws<ArgumentException>(() =>
            order.GetNext(new Side("test:outsider")));
    }

    [Fact]
    public void PseudoLegalGenerator_RequiresOccupiedCurrentSideOrigin()
    {
        var from = TestSupport.At(1, 0, 2);
        var destination = TestSupport.At(0, 0, 2);
        var definition = new PieceDefinition(
            new PieceDefinitionId("test:mover"),
            "Mover",
            new SlidingMovementPattern(
                CompassDirections.North,
                maxDistance: 1));
        var currentPiece = TestSupport.Piece(definition: definition);
        var otherPiece = TestSupport.Piece(TestSupport.Black, definition);
        var state = TestSupport.CreateState(
            TestSupport.CreateGrid(2, 2),
            null,
            (from, currentPiece),
            (TestSupport.At(1, 1, 2), otherPiece));
        var generator = new PseudoLegalGameMoveGenerator();

        Assert.Equal(
            [new Move(from, destination)],
            generator.GenerateMoves(state, from));
        Assert.Empty(generator.GenerateMoves(state, TestSupport.At(0, 1, 2)));
        Assert.Empty(generator.GenerateMoves(state, TestSupport.At(1, 1, 2)));
    }

    [Fact]
    public void Executor_RejectsEmptyWrongSideAndIllegalOrigins()
    {
        var topology = TestSupport.CreateGrid(1, 3);
        var whiteSquare = TestSupport.At(0, 0, 3);
        var blackSquare = TestSupport.At(0, 1, 3);
        var emptySquare = TestSupport.At(0, 2, 3);
        var allowed = new Move(whiteSquare, blackSquare);
        var state = TestSupport.CreateState(
            topology,
            null,
            (whiteSquare, TestSupport.Piece()),
            (blackSquare, TestSupport.Piece(TestSupport.Black)));
        var executor = new GameMoveExecutor(
            new GameMoveResolver(
                new FixedMoveGenerator(allowed),
                new BasicMoveExecutionResolver()));

        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, new Move(emptySquare, whiteSquare)));
        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, new Move(blackSquare, emptySquare)));
        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, new Move(whiteSquare, emptySquare)));
    }

    [Fact]
    public void Executor_RejectsResolverMismatchWithoutChangingState()
    {
        var topology = TestSupport.CreateGrid(1, 3);
        var from = TestSupport.At(0, 0, 3);
        var to = TestSupport.At(0, 1, 3);
        var different = TestSupport.At(0, 2, 3);
        var piece = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(topology, null, (from, piece));
        var before = GameStateSnapshot.Capture(state);
        var executor = new GameMoveExecutor(
            new GameMoveResolver(
                new FixedMoveGenerator(move),
                new DelegateResolver((_, _) => new MoveExecution(
                    new Move(from, different),
                    new BoardTransition(
                        new BoardSquareChange(from, piece, null),
                        new BoardSquareChange(to, null, piece))))));

        Assert.Throws<InvalidOperationException>(() =>
            executor.Execute(state, move));
        before.AssertMatches(state);
    }

    [Fact]
    public void Executor_RejectsUndoWithEmptyHistory()
    {
        var state = TestSupport.CreateState(TestSupport.CreateGrid(1, 1));
        var executor = new GameMoveExecutor(
            new GameMoveResolver(
                new FixedMoveGenerator(),
                new BasicMoveExecutionResolver()));

        Assert.Throws<InvalidOperationException>(() =>
            executor.UndoLastMove(state));
    }

    [Fact]
    public void Executor_DelegatesNullStateValidationToMoveResolver()
    {
        var executor = new GameMoveExecutor(
            new GameMoveResolver(
                new FixedMoveGenerator(),
                new BasicMoveExecutionResolver()));

        Assert.Throws<ArgumentNullException>(() =>
            executor.Execute(null!, default));
    }

    [Fact]
    public void CompositeResolver_RequiresExactlyOneMatchingResolver()
    {
        var move = new Move(new Square(1), new Square(2));
        var state = TestSupport.CreateState(TestSupport.CreateGrid(1, 1));
        var never = new DelegateResolver(
            (_, _) => throw new InvalidOperationException(),
            _ => false);
        var noMatch = new CompositeMoveExecutionResolver(never);

        Assert.False(noMatch.CanResolve(move));
        Assert.Throws<InvalidOperationException>(() =>
            noMatch.Resolve(state, move));

        var first = new DelegateResolver((_, _) =>
            throw new InvalidOperationException());
        var ambiguous = new CompositeMoveExecutionResolver(first, first);

        Assert.True(ambiguous.CanResolve(move));
        Assert.Throws<InvalidOperationException>(() =>
            ambiguous.Resolve(state, move));
        Assert.Throws<ArgumentException>(() =>
            new CompositeMoveExecutionResolver());
    }

    [Fact]
    public void BasicResolver_EnforcesItsPublicExecutionContract()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var resolver = new BasicMoveExecutionResolver();

        Assert.False(
            resolver.CanResolve(
                new Move(from, to, new MoveOptionId("test:special"))));

        var empty = TestSupport.CreateState(topology);
        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(empty, new Move(from, to)));

        var friendly = TestSupport.CreateState(
            topology,
            null,
            (from, TestSupport.Piece()),
            (to, TestSupport.Piece()));
        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(friendly, new Move(from, to)));
        Assert.Throws<InvalidOperationException>(() => resolver.Resolve(
            friendly,
            new Move(from, to, new MoveOptionId("test:special"))));
    }

    [Fact]
    public void MoveAndDisplacement_EnforceStructuralIdentity()
    {
        Assert.Throws<ArgumentException>(() => new Move(
            new Square(1),
            new Square(1)));

        var north = new DisplacementComponent(CompassDirections.North, 2);
        var east = new DisplacementComponent(CompassDirections.East, 1);
        var first = new Displacement(north, east);
        var reordered = new Displacement(east, north);
        var different = new Displacement(north);

        Assert.Equal(first, reordered);
        Assert.Equal(first.GetHashCode(), reordered.GetHashCode());
        Assert.NotEqual(first, different);
        Assert.False(first.Equals(null));
    }

    [Fact]
    public void Simulator_RestoresExactStateAfterSuccessfulEvaluation()
    {
        var (state, move, simulator) = CreateSimulation();
        var before = GameStateSnapshot.Capture(state);

        var occupiedDuringEvaluation = simulator.Evaluate(
            state,
            move,
            (simulated, _) => simulated.BoardState.IsOccupied(move.To) &&
                              !simulated.BoardState.IsOccupied(move.From));

        Assert.True(occupiedDuringEvaluation);
        before.AssertMatches(state);
    }

    [Fact]
    public void Simulator_RestoresExactStateWhenEvaluatorThrows()
    {
        var (state, move, simulator) = CreateSimulation();
        var before = GameStateSnapshot.Capture(state);

        Assert.Throws<TestException>(() => simulator.Evaluate<bool>(
            state,
            move,
            (_, _) => throw new TestException()));

        before.AssertMatches(state);
    }

    [Fact]
    public void Simulator_RejectsResolverMismatchWithoutChangingState()
    {
        var topology = TestSupport.CreateGrid(1, 3);
        var from = TestSupport.At(0, 0, 3);
        var to = TestSupport.At(0, 1, 3);
        var different = TestSupport.At(0, 2, 3);
        var piece = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(topology, null, (from, piece));
        var before = GameStateSnapshot.Capture(state);
        var transition = new BoardTransition(
            new BoardSquareChange(from, piece, null),
            new BoardSquareChange(to, null, piece));
        var simulator = new GameMoveSimulator(
            new DelegateResolver((_, _) => new MoveExecution(
                new Move(from, different),
                transition)));

        Assert.Throws<InvalidOperationException>(() =>
            simulator.Evaluate(state, move, (_, _) => true));
        before.AssertMatches(state);
    }

    [Fact]
    public void Game_PreventsExecutionWhenStatusIsTerminal()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var move = new Move(from, to);
        var variant = new GameVariantDefinition(
            new GameVariantId("test:terminal"),
            "Terminal game",
            topology,
            new TurnOrder(TestSupport.White, TestSupport.Black),
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            new FixedMoveGenerator(move),
            new BasicMoveExecutionResolver(),
            new FixedStatusEvaluator(
                new GameStatus(
                    new GameStatusId("test:over"),
                    isTerminal: true,
                    TestSupport.Black)),
            new InitialPiecePlacement(
                from,
                TestSupport.White,
                TestSupport.Token));
        var game = variant.CreateGame();

        Assert.True(game.Status.IsTerminal);
        Assert.Throws<InvalidOperationException>(() => game.Execute(move));
        Assert.True(game.BoardState.IsOccupied(from));
        Assert.Empty(game.State.History);
    }

    [Fact]
    public void ImportantStatusAndVariantInvariantsAreValidated()
    {
        Assert.Throws<ArgumentException>(() => new GameStatus(
            new GameStatusId("test:active"),
            isTerminal: false,
            TestSupport.White));
        Assert.Throws<ArgumentException>(() => new GameStatus(
            new GameStatusId("test:won"),
            isTerminal: true,
            TestSupport.White,
            TestSupport.White));

        var topology = TestSupport.CreateGrid(1, 1);
        var square = TestSupport.At(0, 0, 1);
        var placement = new InitialPiecePlacement(
            square,
            TestSupport.White,
            TestSupport.Token);

        Assert.Throws<ArgumentException>(() => CreateVariant(
            topology,
            placement,
            placement));
        Assert.Throws<ArgumentException>(() => CreateVariant(
            topology,
            new InitialPiecePlacement(
                new Square(10),
                TestSupport.White,
                TestSupport.Token)));
        Assert.Throws<ArgumentException>(() => CreateVariant(
            topology,
            new InitialPiecePlacement(
                square,
                new Side("test:outsider"),
                TestSupport.Token)));
    }

    private static (GameState State, Move Move, GameMoveSimulator Simulator)
        CreateSimulation()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var piece = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(topology, null, (from, piece));

        return (state, move,
            new GameMoveSimulator(new BasicMoveExecutionResolver()));
    }

    private static GameVariantDefinition CreateVariant(
        BoardTopology topology,
        params InitialPiecePlacement[] placements)
    {
        return new GameVariantDefinition(
            new GameVariantId("test:variant"),
            "Test variant",
            topology,
            new TurnOrder(TestSupport.White, TestSupport.Black),
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver(),
            new FixedMoveGenerator(),
            new BasicMoveExecutionResolver(),
            new FixedStatusEvaluator(
                new GameStatus(
                    new GameStatusId("test:active"),
                    isTerminal: false)),
            placements);
    }

    private sealed class TestException : Exception;
}
