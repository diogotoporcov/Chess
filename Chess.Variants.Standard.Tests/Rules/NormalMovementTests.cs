// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class NormalMovementTests
{
    [Theory]
    [InlineData("a2", "a3")]
    [InlineData("a2", "a4")]
    [InlineData("g1", "f3")]
    [InlineData("g1", "h3")]
    public void InitialPawnAndKnightMovesAreLegal(
        string from,
        string to)
    {
        var game = Variant.CreateGame();

        Assert.Contains(
            new Move(TestSupport.Square(from), TestSupport.Square(to)),
            game.GenerateMoves(TestSupport.Square(from)));
    }

    [Theory]
    [InlineData("rook", "d4", "d5")]
    [InlineData("bishop", "d4", "e5")]
    [InlineData("queen", "d4", "a4")]
    [InlineData("king", "a1", "b2")]
    [InlineData("knight", "a1", "b3")]
    public void PiecesHaveRepresentativeOpenMovement(
        string pieceName,
        string from,
        string to)
    {
        var definition = ResolveDefinition(pieceName);
        var placements = new List<Placement>
        {
            TestSupport.At(
                "h8",
                SideDefinitions.Black,
                PieceDefinitions.King),
            TestSupport.At(from, SideDefinitions.White, definition)
        };

        if (definition != PieceDefinitions.King)
        {
            placements.Add(
                TestSupport.At(
                    "h1",
                    SideDefinitions.White,
                    PieceDefinitions.King));
        }

        var game = TestSupport.CreateGame(placements.ToArray());

        Assert.Contains(
            new Move(TestSupport.Square(from), TestSupport.Square(to)),
            game.GenerateMoves(TestSupport.Square(from)));
    }

    [Fact]
    public void SlidingPiece_StopsAtFriendlyPieceAndCapturesEnemyThenStops()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("d4", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("d6", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At(
                "f4",
                SideDefinitions.Black,
                PieceDefinitions.Bishop));
        var moves = game
            .GenerateMoves(TestSupport.Square("d4"))
            .ToArray();

        Assert.Contains(
            new Move(TestSupport.Square("d4"), TestSupport.Square("d5")),
            moves);
        Assert.DoesNotContain(
            new Move(TestSupport.Square("d4"), TestSupport.Square("d6")),
            moves);
        Assert.Contains(
            new Move(TestSupport.Square("d4"), TestSupport.Square("f4")),
            moves);
        Assert.DoesNotContain(
            new Move(TestSupport.Square("d4"), TestSupport.Square("g4")),
            moves);
    }

    [Fact]
    public void Knight_JumpsBlockersButCannotLandOnFriendlyPiece()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At(
                "d4",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At("d5", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("e4", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("c6", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("e6", SideDefinitions.Black, PieceDefinitions.Pawn));
        var moves = game
            .GenerateMoves(TestSupport.Square("d4"))
            .ToArray();

        Assert.DoesNotContain(
            new Move(TestSupport.Square("d4"), TestSupport.Square("c6")),
            moves);
        Assert.Contains(
            new Move(TestSupport.Square("d4"), TestSupport.Square("e6")),
            moves);
        Assert.Contains(
            new Move(TestSupport.Square("d4"), TestSupport.Square("b5")),
            moves);
    }

    [Fact]
    public void CurrentSideRestrictionAndIllegalMovementAreEnforced()
    {
        var game = Variant.CreateGame();

        Assert.Empty(game.GenerateMoves(TestSupport.Square("b8")));
        Assert.DoesNotContain(
            new Move(TestSupport.Square("b1"), TestSupport.Square("b3")),
            game.GenerateMoves(TestSupport.Square("b1")));
        Assert.Throws<InvalidOperationException>(() =>
            game.Execute(
                new Move(TestSupport.Square("b1"), TestSupport.Square("b3"))));
    }

    private static PieceDefinition ResolveDefinition(
        string name)
    {
        return name switch
        {
            "rook" => PieceDefinitions.Rook,
            "bishop" => PieceDefinitions.Bishop,
            "queen" => PieceDefinitions.Queen,
            "king" => PieceDefinitions.King,
            "knight" => PieceDefinitions.Knight,
            _ => throw new ArgumentOutOfRangeException(nameof(name))
        };
    }
}
