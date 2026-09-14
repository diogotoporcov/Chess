// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class InitialPositionTests
{
    [Fact]
    public void Board_Has64SquaresAndCorrectCoordinateMapping()
    {
        var game = Variant.CreateGame();

        Assert.Equal(64, game.BoardState.Topology.Squares.Count);
        Assert.Equal(TestSupport.Square("a8"), BoardLayout.GetSquare(0, 0));
        Assert.Equal(TestSupport.Square("h1"), BoardLayout.GetSquare(7, 7));
        Assert.Equal(7, BoardLayout.GetRow(TestSupport.Square("d1")));
        Assert.Equal(3, BoardLayout.GetColumn(TestSupport.Square("d1")));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BoardLayout.GetSquare(8, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BoardLayout.GetRow(new Square(64)));
    }

    [Fact]
    public void InitialPosition_HasExactPiecesSidesAndDefinitions()
    {
        var game = Variant.CreateGame();
        var expectedBackRank = new[]
        {
            PieceDefinitions.Rook, PieceDefinitions.Knight,
            PieceDefinitions.Bishop, PieceDefinitions.Queen,
            PieceDefinitions.King, PieceDefinitions.Bishop,
            PieceDefinitions.Knight, PieceDefinitions.Rook
        };

        Assert.Equal(
            32,
            game.BoardState.Topology.Squares.Count(square =>
                game.BoardState.IsOccupied(square)));

        for (var column = 0; column < 8; column++)
        {
            AssertPiece(
                game,
                0,
                column,
                SideDefinitions.Black,
                expectedBackRank[column]);
            AssertPiece(
                game,
                1,
                column,
                SideDefinitions.Black,
                PieceDefinitions.Pawn);
            AssertPiece(
                game,
                6,
                column,
                SideDefinitions.White,
                PieceDefinitions.Pawn);
            AssertPiece(
                game,
                7,
                column,
                SideDefinitions.White,
                expectedBackRank[column]);
        }
    }

    [Fact]
    public void InitialPosition_WhiteMovesFirstWithExactly20LegalMoves()
    {
        var game = Variant.CreateGame();

        Assert.Same(SideDefinitions.White, game.State.CurrentSide);
        Assert.Equal(
            20,
            TestSupport.AllMoves(game)
                .Count);
        Assert.Empty(game.GenerateMoves(TestSupport.Square("a7")));
    }

    [Fact]
    public void TestCompositionMatchesProductionInitialBehavior()
    {
        var productionGame = Variant.CreateGame();
        var testGame = TestSupport.CreateGame(
            Variant.Definition.TurnOrder,
            Variant
                .Definition
                .InitialPlacements
                .Select(placement => new Placement(
                    placement.Square,
                    placement.Side,
                    placement.Definition))
                .ToArray());
        var productionStatus = productionGame.Status;
        var testStatus = testGame.Status;

        Assert.Same(
            productionGame.State.CurrentSide,
            testGame.State.CurrentSide);
        Assert.Equal(productionStatus.Id, testStatus.Id);
        Assert.False(productionStatus.IsTerminal);
        Assert.False(testStatus.IsTerminal);
        Assert.Null(productionStatus.Outcome);
        Assert.Null(testStatus.Outcome);

        foreach (var square in productionGame.BoardState.Topology.Squares)
        {
            var productionMoves = productionGame
                .GenerateMoves(square)
                .ToArray();
            var testMoves = testGame
                .GenerateMoves(square)
                .ToArray();

            Assert.Empty(productionMoves.Except(testMoves));
            Assert.Empty(testMoves.Except(productionMoves));
        }
    }

    private static void AssertPiece(
        Game game,
        int row,
        int column,
        Side side,
        PieceDefinition definition)
    {
        Assert.True(
            game.BoardState.TryGetPiece(
                BoardLayout.GetSquare(row, column),
                out var piece));
        Assert.Same(side, piece.Side);
        Assert.Same(definition, piece.Definition);
    }
}
