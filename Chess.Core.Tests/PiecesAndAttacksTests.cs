// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games.Attacks;
using Chess.Core.Movement.Patterns;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Core.Tests;

public sealed class PiecesAndAttacksTests
{
    [Fact]
    public void PieceDefinition_RejectsNullMovementPatternEntry()
    {
        IMovementPattern?[] movementPatterns =
        [
            new SlidingMovementPattern(CompassDirections.North), null
        ];

        var exception = Assert.Throws<ArgumentException>(() =>
            new PieceDefinition(
                new PieceDefinitionId("test:null-pattern"),
                "Null pattern",
                movementPatterns));

        Assert.Equal("movementPatterns", exception.ParamName);
    }

    [Fact]
    public void Piece_ComposesPatternsWithoutDuplicateMovesOrAttacks()
    {
        var duplicatePattern = new SlidingMovementPattern(
            CompassDirections.North,
            maxDistance: 1);
        var definition = new PieceDefinition(
            new PieceDefinitionId("test:duplicate-patterns"),
            "Duplicate patterns",
            duplicatePattern,
            duplicatePattern);
        var piece = TestSupport.Piece(definition: definition);
        var from = TestSupport.At(2, 2);
        var state = TestSupport.CreateState(
            TestSupport.CreateGrid(),
            null,
            (from, piece));

        Assert.Single(
            piece.GeneratePseudoLegalMoves(state.MovementContext, from));
        Assert.Single(
            piece.GenerateAttackedSquares(state.MovementContext, from));
    }

    [Fact]
    public void Piece_RequiresItsExactInstanceAtTheOrigin()
    {
        var square = TestSupport.At(0, 0, 1);
        var placed = TestSupport.Piece();
        var unplaced = TestSupport.Piece();
        var state = TestSupport.CreateState(
            TestSupport.CreateGrid(1, 1),
            null,
            (square, placed));

        Assert.Throws<InvalidOperationException>(() => unplaced
            .GeneratePseudoLegalMoves(state.MovementContext, square)
            .ToArray());
        Assert.Throws<ArgumentException>(() => placed
            .GenerateAttackedSquares(state.MovementContext, new Square(20))
            .ToArray());
    }

    [Fact]
    public void PatternAttackGenerator_CombinesPiecesAndEliminatesDuplicates()
    {
        var definition = new PieceDefinition(
            new PieceDefinitionId("test:north-attacker"),
            "North attacker",
            new SlidingMovementPattern(CompassDirections.North));
        var first = TestSupport.Piece(definition: definition);
        var second = TestSupport.Piece(definition: definition);
        var target = TestSupport.At(0, 2);
        var state = TestSupport.CreateState(
            TestSupport.CreateGrid(),
            null,
            (TestSupport.At(2, 2), first),
            (TestSupport.At(1, 2), second));
        var generator = new PatternAttackGenerator();

        var attacked = generator
            .GenerateAttackedSquares(state, TestSupport.White)
            .ToArray();

        Assert.Equal(
            new[] { TestSupport.At(1, 2), target }.OrderBy(square => square.Id),
            attacked.OrderBy(square => square.Id));
        Assert.True(
            generator.IsSquareAttacked(state, target, TestSupport.White));
        Assert.False(
            generator.IsSquareAttacked(
                state,
                TestSupport.At(4, 4),
                TestSupport.White));
    }

    [Fact]
    public void PatternAttackGenerator_ValidatesSideAndSquare()
    {
        var state = TestSupport.CreateState(TestSupport.CreateGrid(1, 1));
        var generator = new PatternAttackGenerator();

        Assert.Throws<ArgumentException>(() => generator
            .GenerateAttackedSquares(state, new Side("outsider"))
            .ToArray());
        Assert.Throws<ArgumentException>(() =>
            generator.IsSquareAttacked(
                state,
                new Square(2),
                TestSupport.White));
    }
}
