// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Scenarios;

public sealed class FullGameScenariosTests
{
    [Fact]
    public void OpeningSequenceTracksTurnsHistoryAndCapture()
    {
        var game = Variant.CreateGame();
        var blackPawn = TestSupport.PieceAt(game, "d7");

        TestSupport.Play(game, "e2", "e4");
        TestSupport.Play(game, "e7", "e5");
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "b8", "c6");
        TestSupport.Play(game, "f1", "b5");
        TestSupport.Play(game, "a7", "a6");
        TestSupport.Play(game, "b5", "c6");
        var capture = TestSupport.Play(game, "d7", "c6");

        Assert.Equal(8, game.State.History.Count);
        Assert.Equal(8, capture.PlyNumber);
        Assert.Same(SideDefinitions.Black, capture.Side);
        Assert.Same(blackPawn, TestSupport.PieceAt(game, "c6"));
        Assert.Same(SideDefinitions.White, game.State.CurrentSide);
    }

    [Fact]
    public void LegalOpeningSequenceCanReachCastling()
    {
        var game = Variant.CreateGame();

        TestSupport.Play(game, "e2", "e4");
        TestSupport.Play(game, "e7", "e5");
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "b8", "c6");
        TestSupport.Play(game, "f1", "e2");
        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "e1", "g1", MoveOptions.CastleKingSide);

        Assert.Same(
            PieceDefinitions.King,
            TestSupport.PieceAt(game, "g1")
                .Definition);
        Assert.Same(
            PieceDefinitions.Rook,
            TestSupport.PieceAt(game, "f1")
                .Definition);
        Assert.Equal(7, game.State.History.Count);
    }

    [Fact]
    public void ExecuteSeveralMovesThenUndoAllRestoresExactInitialState()
    {
        var game = Variant.CreateGame();
        var initial = StandardGameSnapshot.Capture(game);
        var sequence = new[]
        {
            ("d2", "d4"), ("d7", "d5"), ("c1", "f4"), ("g8", "f6"),
            ("e2", "e3"), ("c8", "f5")
        };

        foreach (var (from, to) in sequence)
        {
            TestSupport.Play(game, from, to);
        }

        for (var index = sequence.Length - 1; index >= 0; index--)
        {
            var undone = game.UndoLastMove();
            Assert.Equal(index + 1, undone.PlyNumber);
        }

        initial.AssertMatches(game);
    }
}
