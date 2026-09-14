// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games.Attacks;
using Chess.Core.Games.Status;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Games.Rules;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class InsufficientMatingMaterialTests
{
    [Fact]
    public void KingAgainstKingIsInsufficient()
    {
        Assert.True(
            IsInsufficient(
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void SingleBishopAgainstKingIsInsufficient()
    {
        Assert.True(
            IsInsufficient(
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c3",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void SingleKnightAgainstKingIsInsufficient()
    {
        Assert.True(
            IsInsufficient(
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c3",
                    SideDefinitions.White,
                    PieceDefinitions.Knight),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void BishopsSplitBetweenSidesOnOneColorComplexAreInsufficient()
    {
        Assert.True(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c2",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "f5",
                    SideDefinitions.Black,
                    PieceDefinitions.Bishop)));
    }

    [Fact]
    public void
        SeveralBishopsSplitBetweenSidesOnOneColorComplexAreInsufficient()
    {
        Assert.True(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c2",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "e4",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "g6",
                    SideDefinitions.Black,
                    PieceDefinitions.Bishop)));
    }

    [Fact]
    public void SeveralBishopsForOneSideOnOneColorComplexAreInsufficient()
    {
        Assert.True(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c2",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "e4",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "g6",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void TwoKnightsAgainstKingAreNotInsufficient()
    {
        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "b1",
                    SideDefinitions.White,
                    PieceDefinitions.Knight),
                TestSupport.At(
                    "c3",
                    SideDefinitions.White,
                    PieceDefinitions.Knight),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void KnightAgainstKnightIsNotInsufficient()
    {
        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "b1",
                    SideDefinitions.White,
                    PieceDefinitions.Knight),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "g8",
                    SideDefinitions.Black,
                    PieceDefinitions.Knight)));
    }

    [Fact]
    public void BishopAgainstKnightIsNotInsufficient()
    {
        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c2",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "g8",
                    SideDefinitions.Black,
                    PieceDefinitions.Knight)));
    }

    [Fact]
    public void BishopsOnOppositeColorComplexesAreNotInsufficient()
    {
        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c2",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c3",
                    SideDefinitions.Black,
                    PieceDefinitions.Bishop)));
    }

    [Fact]
    public void BishopPairCoveringBothColorComplexesIsNotInsufficient()
    {
        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "c2",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "c3",
                    SideDefinitions.White,
                    PieceDefinitions.Bishop),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void PawnRookAndQueenPositionsAreNotInsufficient()
    {
        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "a2",
                    SideDefinitions.White,
                    PieceDefinitions.Pawn),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));

        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "a2",
                    SideDefinitions.White,
                    PieceDefinitions.Rook),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));

        Assert.False(
            IsInsufficient(
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "a2",
                    SideDefinitions.White,
                    PieceDefinitions.Queen),
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)));
    }

    [Fact]
    public void InitialPositionIsNotInsufficient()
    {
        Assert.False(
            InsufficientMatingMaterialDetector.IsInsufficient(
                Variant.CreateGame()
                    .State));
    }

    [Fact]
    public void LegalCaptureTransitionsIntoDeadPositionAndUndoRestoresState()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "c3",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At(
                "d4",
                SideDefinitions.Black,
                PieceDefinitions.Knight));
        var before = StandardGameSnapshot.Capture(game);

        Assert.Null(game.Status.Outcome);
        Assert.False(game.Status.IsTerminal);

        TestSupport.Play(game, "c3", "d4");

        var status = game.Status;
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.Equal(TerminationDefinitions.DeadPosition, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);

        game.UndoLastMove();

        before.AssertMatches(game);
        Assert.Null(game.Status.Outcome);
        Assert.False(game.Status.IsTerminal);
    }

    [Fact]
    public void DeadPositionPreventsOtherwiseLegalMoveExecution()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "c3",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));
        var otherwiseLegalMove = TestSupport.FindMove(game, "e1", "d1");
        var status = game.Status;
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.Equal(TerminationDefinitions.DeadPosition, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
        Assert.Throws<InvalidOperationException>(() =>
            game.Execute(otherwiseLegalMove));
    }

    [Fact]
    public void DeadPositionOutcomePreservesCheckStatus()
    {
        var game = TestSupport.CreateGame(
            SideDefinitions.Black,
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "c3",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.King));
        var checkDetector = new CheckDetector(new PatternAttackGenerator());
        var status = game.Status;
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.True(checkDetector.IsInCheck(game.State, SideDefinitions.Black));
        Assert.NotEmpty(TestSupport.AllMoves(game));
        Assert.True(
            InsufficientMatingMaterialDetector.IsInsufficient(game.State));
        Assert.Equal(StatusDefinitions.Check, status.Id);
        Assert.Equal(TerminationDefinitions.DeadPosition, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
    }

    [Fact]
    public void StalematePrecedesDeadPosition()
    {
        var game = TestSupport.CreateGame(
            SideDefinitions.Black,
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("c7", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "b6",
                SideDefinitions.White,
                PieceDefinitions.Bishop));
        var status = game.Status;
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.True(
            InsufficientMatingMaterialDetector.IsInsufficient(game.State));
        Assert.Empty(TestSupport.AllMoves(game));
        Assert.Equal(StatusDefinitions.Stalemate, status.Id);
        Assert.Equal(TerminationDefinitions.Stalemate, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
    }

    [Fact]
    public void DetectorRejectsNullGameState()
    {
        Assert.Throws<ArgumentNullException>(() =>
            InsufficientMatingMaterialDetector.IsInsufficient(null!));
    }

    private static bool IsInsufficient(
        params Placement[] placements)
    {
        var game = TestSupport.CreateGame(placements);

        return InsufficientMatingMaterialDetector.IsInsufficient(game.State);
    }
}
