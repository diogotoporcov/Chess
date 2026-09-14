// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board.Transitions;
using Chess.Core.Games;
using Chess.Core.Movement;

namespace Chess.Core.Tests.Games;

public sealed class GameMoveResolverTests
{
    [Fact]
    public void LegalMoveReturnsExactExecutionWithoutMutatingState()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var to = TestSupport.At(0, 1, 2);
        var piece = TestSupport.Piece();
        var move = new Move(from, to);
        var state = TestSupport.CreateState(topology, null, (from, piece));
        var before = GameStateSnapshot.Capture(state);
        var resolver = new GameMoveResolver(
            new FixedMoveGenerator(move),
            new BasicMoveExecutionResolver());

        var execution = resolver.Resolve(state, move);

        Assert.Equal(move, execution.Move);
        Assert.Same(
            piece,
            Assert.Single(
                    execution.Transition.Changes,
                    change => change.Square == from)
                .Before);
        before.AssertMatches(state);
    }

    [Fact]
    public void EmptyOriginIsRejected()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var move = new Move(TestSupport.At(0, 0, 2), TestSupport.At(0, 1, 2));
        var state = TestSupport.CreateState(topology);
        var resolver = new GameMoveResolver(
            new FixedMoveGenerator(move),
            new BasicMoveExecutionResolver());

        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(state, move));
    }

    [Fact]
    public void WrongSideOriginIsRejected()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var move = new Move(from, TestSupport.At(0, 1, 2));
        var state = TestSupport.CreateState(
            topology,
            null,
            (from, TestSupport.Piece(TestSupport.Black)));
        var resolver = new GameMoveResolver(
            new FixedMoveGenerator(move),
            new BasicMoveExecutionResolver());

        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(state, move));
    }

    [Fact]
    public void IllegalDestinationIsRejected()
    {
        var topology = TestSupport.CreateGrid(1, 2);
        var from = TestSupport.At(0, 0, 2);
        var move = new Move(from, TestSupport.At(0, 1, 2));
        var state = TestSupport.CreateState(
            topology,
            null,
            (from, TestSupport.Piece()));
        var resolver = new GameMoveResolver(
            new FixedMoveGenerator(),
            new BasicMoveExecutionResolver());

        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(state, move));
    }

    [Fact]
    public void MismatchingExecutionIsRejectedWithoutMutatingState()
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
        var resolver = new GameMoveResolver(
            new FixedMoveGenerator(move),
            new DelegateResolver((_, _) => new MoveExecution(
                new Move(from, different),
                transition)));

        Assert.Throws<InvalidOperationException>(() =>
            resolver.Resolve(state, move));
        before.AssertMatches(state);
    }

    [Fact]
    public void ConstructorAndResolveRejectNullDependencies()
    {
        var moveGenerator = new FixedMoveGenerator();
        var executionResolver = new BasicMoveExecutionResolver();
        var resolver = new GameMoveResolver(moveGenerator, executionResolver);

        Assert.Throws<ArgumentNullException>(() =>
            new GameMoveResolver(null!, executionResolver));
        Assert.Throws<ArgumentNullException>(() =>
            new GameMoveResolver(moveGenerator, null!));
        Assert.Throws<ArgumentNullException>(() =>
            resolver.Resolve(null!, default));
    }
}
