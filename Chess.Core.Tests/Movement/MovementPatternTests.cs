// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Movement;
using Chess.Core.Movement.Conditions;
using Chess.Core.Movement.Orientation;
using Chess.Core.Movement.Patterns;
using Chess.Core.Pieces;

namespace Chess.Core.Tests.Movement;

public sealed class MovementPatternTests
{
    [Fact]
    public void SlidingPattern_GeneratesOpenRayToBoundary()
    {
        var from = TestSupport.At(2, 2);
        var context = CreateContext((from, TestSupport.Piece()));
        var pattern = new SlidingMovementPattern(CompassDirections.North);

        var moves = pattern
            .GeneratePseudoLegalMoves(context, from, TestSupport.White)
            .ToArray();

        Assert.Equal(
            [
                new Move(from, TestSupport.At(1, 2)),
                new Move(from, TestSupport.At(0, 2))
            ],
            moves);
    }

    [Fact]
    public void SlidingPattern_RespectsMaximumDistance()
    {
        var from = TestSupport.At(4, 2);
        var context = CreateContext((from, TestSupport.Piece()));
        var pattern = new SlidingMovementPattern(
            CompassDirections.North,
            maxDistance: 2);

        var moves = pattern
            .GeneratePseudoLegalMoves(context, from, TestSupport.White)
            .ToArray();

        Assert.Equal(
            [
                new Move(from, TestSupport.At(3, 2)),
                new Move(from, TestSupport.At(2, 2))
            ],
            moves);
    }

    [Fact]
    public void SlidingPattern_StopsAtFriendlyBlocker()
    {
        var from = TestSupport.At(3, 2);
        var blocker = TestSupport.At(1, 2);
        var context = CreateContext(
            (from, TestSupport.Piece()),
            (blocker, TestSupport.Piece()));
        var pattern = new SlidingMovementPattern(CompassDirections.North);

        var moves = pattern
            .GeneratePseudoLegalMoves(context, from, TestSupport.White)
            .ToArray();

        Assert.Equal([new Move(from, TestSupport.At(2, 2))], moves);
    }

    [Fact]
    public void SlidingPattern_CapturesEnemyThenStops()
    {
        var from = TestSupport.At(4, 2);
        var enemy = TestSupport.At(2, 2);
        var context = CreateContext(
            (from, TestSupport.Piece()),
            (enemy, TestSupport.Piece(TestSupport.Black)));
        var pattern = new SlidingMovementPattern(CompassDirections.North);

        var moves = pattern
            .GeneratePseudoLegalMoves(context, from, TestSupport.White)
            .ToArray();
        var attacks = pattern
            .GenerateAttackedSquares(context, from, TestSupport.White)
            .ToArray();

        Assert.Equal(
            [new Move(from, TestSupport.At(3, 2)), new Move(from, enemy)],
            moves);
        Assert.Equal([TestSupport.At(3, 2), enemy], attacks);
    }

    [Theory]
    [InlineData(MovementTargetMode.MoveOnly, false, true)]
    [InlineData(MovementTargetMode.MoveOnly, true, false)]
    [InlineData(MovementTargetMode.CaptureOnly, false, false)]
    [InlineData(MovementTargetMode.CaptureOnly, true, true)]
    [InlineData(MovementTargetMode.MoveOrCapture, false, true)]
    [InlineData(MovementTargetMode.MoveOrCapture, true, true)]
    public void SlidingPattern_EnforcesDestinationMode(
        MovementTargetMode mode,
        bool occupiedByEnemy,
        bool expected)
    {
        var from = TestSupport.At(1, 1);
        var destination = TestSupport.At(0, 1);
        var placements = new List<(Square, Piece)>
        {
            (from, TestSupport.Piece())
        };

        if (occupiedByEnemy)
        {
            placements.Add((destination, TestSupport.Piece(TestSupport.Black)));
        }

        var context = CreateContext(placements.ToArray());
        var pattern = new SlidingMovementPattern(
            CompassDirections.North,
            1,
            mode);

        var moves = pattern
            .GeneratePseudoLegalMoves(context, from, TestSupport.White)
            .ToArray();

        Assert.Equal(expected, moves.Contains(new Move(from, destination)));
    }

    [Fact]
    public void LeapingPattern_IgnoresIntermediateOccupancyAndFindsDestination()
    {
        var from = TestSupport.At(3, 1);
        var destination = TestSupport.At(1, 2);
        var context = CreateContext(
            (from, TestSupport.Piece()),
            (TestSupport.At(2, 1), TestSupport.Piece()),
            (TestSupport.At(3, 2), TestSupport.Piece()));
        var pattern = new LeapingMovementPattern(
            new Displacement(
                new DisplacementComponent(CompassDirections.North, 2),
                new DisplacementComponent(CompassDirections.East, 1)));

        var moves = pattern
            .GeneratePseudoLegalMoves(context, from, TestSupport.White)
            .ToArray();

        Assert.Equal([new Move(from, destination)], moves);
    }

    [Fact]
    public void LeapingPattern_HandlesBoundaryAndDestinationOccupancy()
    {
        var from = TestSupport.At(0, 0);
        var offBoard = new LeapingMovementPattern(
            new Displacement(
                new DisplacementComponent(CompassDirections.North, 2),
                new DisplacementComponent(CompassDirections.East, 1)));
        var context = CreateContext((from, TestSupport.Piece()));

        Assert.Empty(
            offBoard.GeneratePseudoLegalMoves(
                context,
                from,
                TestSupport.White));

        var center = TestSupport.At(3, 1);
        var destination = TestSupport.At(1, 2);
        var occupiedContext = CreateContext(
            (center, TestSupport.Piece()),
            (destination, TestSupport.Piece()));
        var leap = new LeapingMovementPattern(
            new Displacement(
                new DisplacementComponent(CompassDirections.North, 2),
                new DisplacementComponent(CompassDirections.East, 1)));

        Assert.Empty(
            leap.GeneratePseudoLegalMoves(
                occupiedContext,
                center,
                TestSupport.White));
        Assert.Equal(
            [destination],
            leap
                .GenerateAttackedSquares(
                    occupiedContext,
                    center,
                    TestSupport.White)
                .ToArray());
    }

    [Fact]
    public void MoveOnlyLeapCanMoveButDoesNotAttack()
    {
        var from = TestSupport.At(3, 1);
        var destination = TestSupport.At(1, 2);
        var context = CreateContext((from, TestSupport.Piece()));
        var pattern = new LeapingMovementPattern(
            new Displacement(
                new DisplacementComponent(CompassDirections.North, 2),
                new DisplacementComponent(CompassDirections.East, 1)),
            MovementTargetMode.MoveOnly);

        Assert.Equal(
            [new Move(from, destination)],
            pattern.GeneratePseudoLegalMoves(context, from, TestSupport.White));
        Assert.Empty(
            pattern.GenerateAttackedSquares(context, from, TestSupport.White));
    }

    [Fact]
    public void PathPattern_RequiresEveryOrderedIntermediateSquareToBeOpen()
    {
        var from = TestSupport.At(3, 2);
        var destination = TestSupport.At(1, 3);
        var pattern = new PathMovementPattern(
            CompassDirections.North,
            CompassDirections.North,
            CompassDirections.East);
        var open = CreateContext((from, TestSupport.Piece()));
        var blocked = CreateContext(
            (from, TestSupport.Piece()),
            (TestSupport.At(2, 2), TestSupport.Piece()));

        Assert.Equal(
            [new Move(from, destination)],
            pattern.GeneratePseudoLegalMoves(open, from, TestSupport.White));
        Assert.Empty(
            pattern.GeneratePseudoLegalMoves(blocked, from, TestSupport.White));
        Assert.Empty(
            pattern.GenerateAttackedSquares(blocked, from, TestSupport.White));
    }

    [Fact]
    public void PathPattern_AppliesTargetSemanticsOnlyAtDestination()
    {
        var from = TestSupport.At(3, 2);
        var destination = TestSupport.At(1, 2);
        var pattern = new PathMovementPattern(
            MovementTargetMode.CaptureOnly,
            CompassDirections.North,
            CompassDirections.North);
        var empty = CreateContext((from, TestSupport.Piece()));
        var enemy = CreateContext(
            (from, TestSupport.Piece()),
            (destination, TestSupport.Piece(TestSupport.Black)));

        Assert.Empty(
            pattern.GeneratePseudoLegalMoves(empty, from, TestSupport.White));
        Assert.Equal(
            [new Move(from, destination)],
            pattern.GeneratePseudoLegalMoves(enemy, from, TestSupport.White));
        Assert.Equal(
            [destination],
            pattern.GenerateAttackedSquares(empty, from, TestSupport.White));
    }

    [Fact]
    public void PathPattern_StopsAtBoundaryAndFriendlyDestination()
    {
        var edge = TestSupport.At(0, 0);
        var boundary = CreateContext((edge, TestSupport.Piece()));
        var northPath = new PathMovementPattern(
            CompassDirections.North,
            CompassDirections.North);

        Assert.Empty(
            northPath.GeneratePseudoLegalMoves(
                boundary,
                edge,
                TestSupport.White));
        Assert.Empty(
            northPath.GenerateAttackedSquares(
                boundary,
                edge,
                TestSupport.White));

        var from = TestSupport.At(2, 2);
        var destination = TestSupport.At(0, 2);
        var friendly = CreateContext(
            (from, TestSupport.Piece()),
            (destination, TestSupport.Piece()));

        Assert.Empty(
            northPath.GeneratePseudoLegalMoves(
                friendly,
                from,
                TestSupport.White));
        Assert.Equal(
            [destination],
            northPath.GenerateAttackedSquares(
                friendly,
                from,
                TestSupport.White));
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void ConditionalPattern_DelegatesOnlyWhenEnabled(
        bool enabled,
        int expectedMoveCount)
    {
        var from = TestSupport.At(2, 2);
        var context = CreateContext((from, TestSupport.Piece()));
        var pattern = new ConditionalMovementPattern(
            new FixedCondition(enabled),
            new SlidingMovementPattern(
                CompassDirections.North,
                maxDistance: 1));

        Assert.Equal(
            expectedMoveCount,
            pattern
                .GeneratePseudoLegalMoves(context, from, TestSupport.White)
                .Count());
        Assert.Equal(
            expectedMoveCount,
            pattern
                .GenerateAttackedSquares(context, from, TestSupport.White)
                .Count());
    }

    [Fact]
    public void RelativeDirection_UsesTheMovingSidesOrientation()
    {
        var from = TestSupport.At(2, 2);
        var piece = TestSupport.Piece();
        var topology = TestSupport.CreateGrid();
        var board = new BoardStateBuilder(topology)
            .PlacePiece(from, piece)
            .Build();
        var forward = new RelativeDirection("forward");
        var context = new MovementContext(
            board,
            new SideOrientationMap(
                (TestSupport.White, forward, CompassDirections.North),
                (TestSupport.Black, forward, CompassDirections.South)),
            new EmptyRegionResolver());
        var pattern = new SlidingMovementPattern(forward, 1);

        Assert.Equal(
            [new Move(from, TestSupport.At(1, 2))],
            pattern.GeneratePseudoLegalMoves(context, from, TestSupport.White));
    }

    [Fact]
    public void OriginInRegionCondition_UsesSideSpecificMembership()
    {
        var from = TestSupport.At(2, 2);
        var topology = TestSupport.CreateGrid();
        var board = new BoardStateBuilder(topology).Build();
        var regionId = new BoardRegionId("start");
        var context = new MovementContext(
            board,
            new ThrowingOrientationResolver(),
            new SideBoardRegionMap(
                (TestSupport.White, regionId, new BoardRegion([from])),
                (TestSupport.Black, regionId,
                    new BoardRegion([TestSupport.At(1, 1)]))));
        var condition = new OriginInRegionCondition(regionId);

        Assert.True(condition.IsSatisfied(context, from, TestSupport.White));
        Assert.False(condition.IsSatisfied(context, from, TestSupport.Black));
        Assert.Throws<ArgumentException>(() => context.IsInRegion(
            TestSupport.White,
            regionId,
            new Square(100)));
    }

    [Fact]
    public void MovementConstructors_RejectInvalidStructuralValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SlidingMovementPattern(CompassDirections.North, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LeapingMovementPattern(
                new Displacement(
                    new DisplacementComponent(CompassDirections.North, 1)),
                (MovementTargetMode)99));
        Assert.Throws<ArgumentException>(() => new PathMovementPattern());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new DisplacementComponent(CompassDirections.North, 0));
        Assert.Throws<ArgumentException>(() => new Displacement());
        Assert.Throws<ArgumentException>(() => new Displacement(
            new DisplacementComponent(CompassDirections.North, 1),
            new DisplacementComponent(CompassDirections.North, 2)));
    }

    private static MovementContext CreateContext(
        params (Square Square, Piece Piece)[] placements)
    {
        return TestSupport.CreateState(
                TestSupport.CreateGrid(),
                null,
                placements)
            .MovementContext;
    }
}
