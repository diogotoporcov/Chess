// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Perft;

public sealed class PerftTests
{
    [Theory]
    [InlineData(0, 1L)]
    [InlineData(1, 20L)]
    [InlineData(2, 400L)]
    [InlineData(3, 8_902L)]
    public void InitialPosition_NormalDepthsMatchCanonicalCounts(
        int depth,
        long expectedNodes)
    {
        var game = Variant.CreateGame();

        Assert.Equal(expectedNodes, CountNodes(game, depth));
    }

    [Fact]
    [Trait("Category", "Slow")]
    public void InitialPosition_Depth4MatchesCanonicalCount()
    {
        Assert.Equal(197_281, CountNodes(Variant.CreateGame(), 4));
    }

    [Fact(Explicit = true)]
    [Trait("Category", "Manual")]
    public void InitialPosition_Depth5MatchesCanonicalCount()
    {
        Assert.Equal(4_865_609, CountNodes(Variant.CreateGame(), 5));
    }

    [Theory]
    [InlineData(1, 48L)]
    [InlineData(2, 2_039L)]
    public void Kiwipete_NormalDepthsMatchCanonicalCounts(
        int depth,
        long expectedNodes)
    {
        Assert.Equal(expectedNodes, CountNodes(CreateKiwipete(), depth));
    }

    [Fact]
    [Trait("Category", "Slow")]
    public void Kiwipete_Depth3MatchesCanonicalCount()
    {
        Assert.Equal(97_862, CountNodes(CreateKiwipete(), 3));
    }

    [Fact(Explicit = true)]
    [Trait("Category", "Manual")]
    public void Kiwipete_Depth4MatchesCanonicalCount()
    {
        Assert.Equal(4_085_603, CountNodes(CreateKiwipete(), 4));
    }

    private static long CountNodes(
        Game game,
        int depth)
    {
        if (depth == 0)
        {
            return 1;
        }

        long nodes = 0;
        var moves = TestSupport.AllMoves(game);

        foreach (var move in moves)
        {
            game.Execute(move);
            nodes += CountNodes(game, depth - 1);
            game.UndoLastMove();
        }

        return nodes;
    }

    private static Game CreateKiwipete()
    {
        return TestSupport.CreateGame(
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.Rook),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.Rook),
            TestSupport.At("a7", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At("c7", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At("d7", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At("e7", SideDefinitions.Black, PieceDefinitions.Queen),
            TestSupport.At("f7", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At(
                "g7",
                SideDefinitions.Black,
                PieceDefinitions.Bishop),
            TestSupport.At(
                "a6",
                SideDefinitions.Black,
                PieceDefinitions.Bishop),
            TestSupport.At(
                "b6",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At("e6", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At(
                "f6",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At("g6", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At("b4", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At("d5", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                "e5",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At("e4", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                "c3",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At("f3", SideDefinitions.White, PieceDefinitions.Queen),
            TestSupport.At("h3", SideDefinitions.Black, PieceDefinitions.Pawn),
            TestSupport.At("a2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("b2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("c2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                "d2",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At(
                "e2",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("f2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("g2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("h2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook));
    }
}
