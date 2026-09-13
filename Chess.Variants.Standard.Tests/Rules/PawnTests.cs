// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class PawnTests
{
    [Fact]
    public void WhitePawn_MovesForwardAndCapturesOnlyDiagonally()
    {
        var game = TestSupport.CreateGame(
        [
            .. Kings(),
            TestSupport.At("d4", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                "c5",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At("e5", SideDefinitions.Black, PieceDefinitions.Bishop)
        ]);
        var moves = game
            .GenerateMoves(TestSupport.Square("d4"))
            .ToArray();

        Assert.Contains(Move("d4", "d5"), moves);
        Assert.Contains(Move("d4", "c5"), moves);
        Assert.Contains(Move("d4", "e5"), moves);
        Assert.DoesNotContain(Move("d4", "c3"), moves);
    }

    [Fact]
    public void Pawn_CannotMoveDiagonallyToEmptySquareOrCaptureForward()
    {
        var game = TestSupport.CreateGame(
        [
            .. Kings(),
            TestSupport.At("d4", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("d5", SideDefinitions.Black, PieceDefinitions.Knight)
        ]);
        var moves = game
            .GenerateMoves(TestSupport.Square("d4"))
            .ToArray();

        Assert.DoesNotContain(Move("d4", "c5"), moves);
        Assert.DoesNotContain(Move("d4", "e5"), moves);
        Assert.DoesNotContain(Move("d4", "d5"), moves);
    }

    [Theory]
    [InlineData("a3")]
    [InlineData("a4")]
    public void InitialDoubleAdvance_IsBlockedByEitherRequiredSquare(
        string blockedSquare)
    {
        var game = TestSupport.CreateGame(
        [
            .. Kings(),
            TestSupport.At("a2", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                blockedSquare,
                SideDefinitions.Black,
                PieceDefinitions.Knight)
        ]);
        var moves = game
            .GenerateMoves(TestSupport.Square("a2"))
            .ToArray();

        Assert.DoesNotContain(Move("a2", "a4"), moves);

        if (blockedSquare == "a3")
        {
            Assert.DoesNotContain(Move("a2", "a3"), moves);
        }
        else
        {
            Assert.Contains(Move("a2", "a3"), moves);
        }
    }

    [Fact]
    public void BlackPawn_UsesBlackRelativeOrientation()
    {
        var game = TestSupport.CreateGame(
            new TurnOrder(SideDefinitions.Black, SideDefinitions.White),
            [
                .. Kings(),
                TestSupport.At(
                    "d7",
                    SideDefinitions.Black,
                    PieceDefinitions.Pawn),
                TestSupport.At(
                    "c6",
                    SideDefinitions.White,
                    PieceDefinitions.Knight)
            ]);
        var moves = game
            .GenerateMoves(TestSupport.Square("d7"))
            .ToArray();

        Assert.Contains(Move("d7", "d6"), moves);
        Assert.Contains(Move("d7", "d5"), moves);
        Assert.Contains(Move("d7", "c6"), moves);
        Assert.DoesNotContain(Move("d7", "d8"), moves);
    }

    private static Placement[] Kings()
    {
        return
        [
            TestSupport.At(
                "h1",
                SideDefinitions.White,
                PieceDefinitions.King),
            TestSupport.At(
                "h8",
                SideDefinitions.Black,
                PieceDefinitions.King)
        ];
    }

    private static Move Move(
        string from,
        string to)
    {
        return new Move(TestSupport.Square(from), TestSupport.Square(to));
    }
}
