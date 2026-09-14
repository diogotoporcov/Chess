// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Pieces;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class HalfmoveClockTests
{
    [Fact]
    public void InitialClockIsZero()
    {
        Assert.Equal(0, Clock(Variant.CreateGame()));
    }

    [Fact]
    public void QuietNonPawnMovesIncrementClock()
    {
        var game = Variant.CreateGame();

        TestSupport.Play(game, "g1", "f3");
        Assert.Equal(1, Clock(game));

        TestSupport.Play(game, "g8", "f6");
        Assert.Equal(2, Clock(game));
    }

    [Fact]
    public void PawnMoveResetsClockAndFollowingQuietMoveIncrementsFromZero()
    {
        var game = Variant.CreateGame();
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "g8", "f6");

        TestSupport.Play(game, "e2", "e4");
        Assert.Equal(0, Clock(game));

        TestSupport.Play(game, "b8", "c6");
        Assert.Equal(1, Clock(game));
    }

    [Fact]
    public void OrdinaryCaptureResetsClock()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At(
                "a8",
                SideDefinitions.Black,
                PieceDefinitions.Knight));

        TestSupport.Play(game, "a1", "a2");
        TestSupport.Play(game, "e8", "f8");
        Assert.Equal(2, Clock(game));

        TestSupport.Play(game, "a2", "a8");

        Assert.Equal(0, Clock(game));
    }

    [Fact]
    public void PawnCaptureResetsClock()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e4", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                "d5",
                SideDefinitions.Black,
                PieceDefinitions.Knight));

        TestSupport.Play(game, "e1", "f1");
        TestSupport.Play(game, "e8", "f8");
        Assert.Equal(2, Clock(game));

        TestSupport.Play(game, "e4", "d5");

        Assert.Equal(0, Clock(game));
    }

    [Fact]
    public void EnPassantResetsClock()
    {
        var game = Variant.CreateGame();
        TestSupport.Play(game, "e2", "e4");
        TestSupport.Play(game, "a7", "a6");
        TestSupport.Play(game, "e4", "e5");
        TestSupport.Play(game, "d7", "d5");

        TestSupport.Play(game, "e5", "d6", MoveOptions.EnPassant);

        Assert.Equal(0, Clock(game));
    }

    [Fact]
    public void NonCapturingPromotionResetsClock()
    {
        var game = CreatePromotionGame(capturablePiece: null);

        AccumulateTwoQuietMoves(game);
        TestSupport.Play(game, "a7", "a8", PromotionOptions.Queen);

        Assert.Equal(0, Clock(game));
    }

    [Fact]
    public void CapturingPromotionResetsClock()
    {
        var game = CreatePromotionGame(PieceDefinitions.Rook);

        AccumulateTwoQuietMoves(game);
        TestSupport.Play(game, "a7", "b8", PromotionOptions.Knight);

        Assert.Equal(0, Clock(game));
    }

    [Fact]
    public void CastlingIncrementsClock()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King));

        TestSupport.Play(game, "e1", "g1", MoveOptions.CastleKingSide);

        Assert.Equal(1, Clock(game));
    }

    [Fact]
    public void UndoAndMultipleUndosRestoreEveryEarlierClock()
    {
        var game = Variant.CreateGame();

        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "e2", "e4");
        TestSupport.Play(game, "b8", "c6");
        Assert.Equal(1, Clock(game));

        game.UndoLastMove();
        Assert.Equal(0, Clock(game));

        game.UndoLastMove();
        Assert.Equal(2, Clock(game));

        game.UndoLastMove();
        Assert.Equal(1, Clock(game));

        game.UndoLastMove();
        Assert.Equal(0, Clock(game));
    }

    private static int Clock(
        Game game)
    {
        return TestSupport
            .CreatePositionFactsEvaluator(game.Variant)
            .EvaluateHalfmoveClock(game.State);
    }

    private static Game CreatePromotionGame(
        PieceDefinition? capturablePiece)
    {
        var placements = new List<Placement>
        {
            TestSupport.At(
                "e1",
                SideDefinitions.White,
                PieceDefinitions.King),
            TestSupport.At(
                "e8",
                SideDefinitions.Black,
                PieceDefinitions.King),
            TestSupport.At(
                "a7",
                SideDefinitions.White,
                PieceDefinitions.Pawn)
        };

        if (capturablePiece is not null)
        {
            placements.Add(
                TestSupport.At("b8", SideDefinitions.Black, capturablePiece));
        }

        return TestSupport.CreateGame(placements.ToArray());
    }

    private static void AccumulateTwoQuietMoves(
        Game game)
    {
        TestSupport.Play(game, "e1", "f1");
        TestSupport.Play(game, "e8", "f8");
        Assert.Equal(2, Clock(game));
    }
}
