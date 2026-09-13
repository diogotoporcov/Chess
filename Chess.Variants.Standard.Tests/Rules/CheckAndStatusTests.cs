// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Attacks;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Games.Rules;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class CheckAndStatusTests
{
    [Theory]
    [InlineData("rook", "d8")]
    [InlineData("bishop", "a7")]
    [InlineData("queen", "d8")]
    [InlineData("knight", "c6")]
    [InlineData("pawn", "c5")]
    [InlineData("king", "e5")]
    public void CheckIsDetectedFromEveryRelevantPieceType(
        string attackerName,
        string attackerSquare)
    {
        var attacker = ResolveDefinition(attackerName);
        var placements = new List<Placement>
        {
            TestSupport.At(
                "d4",
                SideDefinitions.White,
                PieceDefinitions.King),
            TestSupport.At(attackerSquare, SideDefinitions.Black, attacker)
        };

        if (attacker != PieceDefinitions.King)
        {
            placements.Add(
                TestSupport.At(
                    "h8",
                    SideDefinitions.Black,
                    PieceDefinitions.King));
        }

        var game = TestSupport.CreateGame(placements.ToArray());

        Assert.Equal(StatusDefinitions.Check, game.Status.Id);
        Assert.False(game.Status.IsTerminal);
    }

    [Fact]
    public void PinnedPieceCannotExposeItsKing()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e2", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.Rook));
        var moves = game
            .GenerateMoves(TestSupport.Square("e2"))
            .ToArray();

        Assert.Contains(Move("e2", "e3"), moves);
        Assert.DoesNotContain(Move("e2", "d2"), moves);
        Assert.DoesNotContain(Move("e2", "f2"), moves);
    }

    [Fact]
    public void KingCannotMoveIntoAnAttackedSquare()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("d8", SideDefinitions.Black, PieceDefinitions.Rook));

        Assert.DoesNotContain(
            Move("e1", "d1"),
            game.GenerateMoves(TestSupport.Square("e1")));
    }

    [Fact]
    public void KingCannotCaptureOntoASquareDefendedByAnotherPiece()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e4", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At(
                "d5",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At("d8", SideDefinitions.Black, PieceDefinitions.Rook));

        Assert.DoesNotContain(
            Move("e4", "d5"),
            game.GenerateMoves(TestSupport.Square("e4")));
    }

    [Fact]
    public void BlockingACheckingRayIsALegalEvasion()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "f1",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.Rook));

        Assert.Equal(StatusDefinitions.Check, game.Status.Id);
        Assert.Contains(
            Move("f1", "e2"),
            game.GenerateMoves(TestSupport.Square("f1")));
    }

    [Fact]
    public void KingIsNeverGeneratedAsAnOrdinaryCapture()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e7", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));

        Assert.DoesNotContain(
            Move("e7", "e8"),
            game.GenerateMoves(TestSupport.Square("e7")));
    }

    [Fact]
    public void InitialPositionIsActive()
    {
        var status = Variant.CreateGame()
            .Status;

        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.False(status.IsTerminal);
        Assert.Empty(status.Winners);
    }

    [Fact]
    public void FoolMateIsCheckmateAndPreventsFurtherExecution()
    {
        var game = CreateFoolsMate();
        var status = game.Status;

        Assert.Equal(StatusDefinitions.Checkmate, status.Id);
        Assert.True(status.IsTerminal);
        Assert.Equal([SideDefinitions.Black], status.Winners);
        Assert.Empty(TestSupport.AllMoves(game));
        Assert.Throws<InvalidOperationException>(() =>
            game.Execute(Move("e2", "e3")));
    }

    [Fact]
    public void KnownPositionIsStalemateWithoutWinner()
    {
        var game = TestSupport.CreateGame(
            new TurnOrder(SideDefinitions.Black, SideDefinitions.White),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("c6", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "b6",
                SideDefinitions.White,
                PieceDefinitions.Queen));
        var status = game.Status;

        Assert.Equal(StatusDefinitions.Stalemate, status.Id);
        Assert.True(status.IsTerminal);
        Assert.Empty(status.Winners);
        Assert.Empty(TestSupport.AllMoves(game));
    }

    [Fact]
    public void CheckDetectorRejectsSideOutsideGameAndRequiresExactlyOneKing()
    {
        var detector = new CheckDetector(new PatternAttackGenerator());
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));

        Assert.Throws<ArgumentException>(() => detector.IsInCheck(
            game.State,
            new Side("test:outsider")));

        var missingKing = TestSupport.CreateGame(
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));

        Assert.Throws<InvalidOperationException>(() =>
            detector.IsInCheck(missingKing.State, SideDefinitions.White));
    }

    internal static Game CreateFoolsMate()
    {
        var game = Variant.CreateGame();
        TestSupport.Play(game, "f2", "f3");
        TestSupport.Play(game, "e7", "e5");
        TestSupport.Play(game, "g2", "g4");
        TestSupport.Play(game, "d8", "h4");
        return game;
    }

    private static Move Move(
        string from,
        string to)
    {
        return new Move(TestSupport.Square(from), TestSupport.Square(to));
    }

    private static PieceDefinition ResolveDefinition(
        string name)
    {
        return name switch
        {
            "rook" => PieceDefinitions.Rook,
            "bishop" => PieceDefinitions.Bishop,
            "queen" => PieceDefinitions.Queen,
            "knight" => PieceDefinitions.Knight,
            "pawn" => PieceDefinitions.Pawn,
            "king" => PieceDefinitions.King,
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };
    }
}
