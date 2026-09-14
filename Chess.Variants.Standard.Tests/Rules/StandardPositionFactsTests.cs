// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Games.History;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class StandardPositionFactsTests
{
    [Fact]
    public void KeyCanonicalizesPlacementOrderAtConstruction()
    {
        var unordered = new[]
        {
            new StandardPiecePlacement(
                new Square(12),
                "chess:black",
                "chess:bishop"),
            new StandardPiecePlacement(
                new Square(3),
                "chess:white",
                "chess:knight")
        };
        var first = new StandardPositionKey(
            unordered,
            "chess:white",
            default,
            null);
        var second = new StandardPositionKey(
            unordered.Reverse(),
            "chess:white",
            default,
            null);

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
        Assert.Equal(new Square(3), first.PiecePlacements[0].Square);
        Assert.Equal(new Square(12), first.PiecePlacements[1].Square);
    }

    [Fact]
    public void KeyRejectsDuplicatePiecePlacementSquares()
    {
        var placements = new[]
        {
            new StandardPiecePlacement(
                new Square(3),
                "chess:white",
                "chess:knight"),
            new StandardPiecePlacement(
                new Square(3),
                "chess:black",
                "chess:bishop")
        };

        Assert.Throws<ArgumentException>(() =>
            new StandardPositionKey(placements, "chess:white", default, null));
    }

    [Fact]
    public void EquivalentIndependentPositionsHaveEqualCanonicalKeys()
    {
        var placements = new[]
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
                "c3",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At(
                "d6",
                SideDefinitions.Black,
                PieceDefinitions.Bishop)
        };
        var first = TestSupport.CreateGame(placements);
        var second = TestSupport.CreateGame(
            placements
                .Reverse()
                .ToArray());

        var firstKey = CreateKey(first);
        var secondKey = CreateKey(second);

        Assert.Equal(firstKey, secondKey);
        Assert.Equal(firstKey.GetHashCode(), secondKey.GetHashCode());
        Assert.All(
            firstKey.PiecePlacements.Zip(secondKey.PiecePlacements),
            pair => Assert.Equal(pair.First, pair.Second));
        Assert.NotSame(
            TestSupport.PieceAt(first, "c3"),
            TestSupport.PieceAt(second, "c3"));
    }

    [Theory]
    [InlineData("c3", "d5")]
    [InlineData("c3", "b5")]
    public void DifferentPieceSquareChangesKey(
        string from,
        string to)
    {
        var first = CreatePieceIdentityGame(
            SideDefinitions.White,
            PieceDefinitions.Knight,
            from);
        var second = CreatePieceIdentityGame(
            SideDefinitions.White,
            PieceDefinitions.Knight,
            to);

        Assert.NotEqual(CreateKey(first), CreateKey(second));
    }

    [Fact]
    public void DifferentPieceDefinitionChangesKey()
    {
        var knight = CreatePieceIdentityGame(
            SideDefinitions.White,
            PieceDefinitions.Knight,
            "c3");
        var bishop = CreatePieceIdentityGame(
            SideDefinitions.White,
            PieceDefinitions.Bishop,
            "c3");

        Assert.NotEqual(CreateKey(knight), CreateKey(bishop));
    }

    [Fact]
    public void DifferentPieceSideChangesKey()
    {
        var white = CreatePieceIdentityGame(
            SideDefinitions.White,
            PieceDefinitions.Knight,
            "c3");
        var black = CreatePieceIdentityGame(
            SideDefinitions.Black,
            PieceDefinitions.Knight,
            "c3");

        Assert.NotEqual(CreateKey(white), CreateKey(black));
    }

    [Fact]
    public void DifferentSideToMoveChangesKey()
    {
        var placements = new[]
        {
            TestSupport.At(
                "e1",
                SideDefinitions.White,
                PieceDefinitions.King),
            TestSupport.At(
                "e8",
                SideDefinitions.Black,
                PieceDefinitions.King)
        };
        var whiteToMove = TestSupport.CreateGame(
            new TurnOrder(SideDefinitions.White, SideDefinitions.Black),
            placements);
        var blackToMove = TestSupport.CreateGame(
            SideDefinitions.Black,
            placements);

        Assert.NotEqual(CreateKey(whiteToMove), CreateKey(blackToMove));
    }

    [Theory]
    [InlineData("h1", "h2", true)]
    [InlineData("a1", "a2", false)]
    public void RookMovesAwayAndReturnsChangesOnlyItsCastlingRight(
        string rookFrom,
        string rookThrough,
        bool kingSide)
    {
        var intact = CreateWhiteRightsGame();
        var lost = CreateWhiteRightsGame();

        TestSupport.Play(lost, rookFrom, rookThrough);
        TestSupport.Play(lost, "e8", "e7");
        TestSupport.Play(lost, rookThrough, rookFrom);
        TestSupport.Play(lost, "e7", "e8");

        var intactFacts = Evaluate(intact);
        var lostFacts = Evaluate(lost);

        Assert.NotEqual(intactFacts.PositionKey, lostFacts.PositionKey);
        Assert.Equal(
            kingSide,
            intactFacts.CastlingRights.WhiteKingSide !=
            lostFacts.CastlingRights.WhiteKingSide);
        Assert.Equal(
            !kingSide,
            intactFacts.CastlingRights.WhiteQueenSide !=
            lostFacts.CastlingRights.WhiteQueenSide);
    }

    [Fact]
    public void KingMovesAwayAndReturnsLosesBothRightsWithoutChangingBoard()
    {
        var intact = CreateWhiteRightsGame();
        var lost = CreateWhiteRightsGame();

        TestSupport.Play(lost, "e1", "f1");
        TestSupport.Play(lost, "e8", "e7");
        TestSupport.Play(lost, "f1", "e1");
        TestSupport.Play(lost, "e7", "e8");

        var lostFacts = Evaluate(lost);

        Assert.NotEqual(CreateKey(intact), lostFacts.PositionKey);
        Assert.False(lostFacts.CastlingRights.WhiteKingSide);
        Assert.False(lostFacts.CastlingRights.WhiteQueenSide);
    }

    [Fact]
    public void BlockingPiecesDoNotRemoveHistoricalCastlingRight()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "f1",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King));

        var facts = Evaluate(game);

        Assert.True(facts.CastlingRights.WhiteKingSide);
        Assert.DoesNotContain(
            TestSupport.Square("g1"),
            game
                .GenerateMoves(TestSupport.Square("e1"))
                .Select(move => move.To));
    }

    [Fact]
    public void CheckDoesNotRemoveHistoricalCastlingRight()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.Rook));

        var facts = Evaluate(game);

        Assert.True(facts.CastlingRights.WhiteKingSide);
        Assert.DoesNotContain(
            new Move(
                TestSupport.Square("e1"),
                TestSupport.Square("g1"),
                MoveOptions.CastleKingSide),
            game.GenerateMoves(TestSupport.Square("e1")));
    }

    [Fact]
    public void UndoRestoresPositionKeyAndCastlingRights()
    {
        var game = CreateWhiteRightsGame();
        var before = Evaluate(game);

        TestSupport.Play(game, "h1", "h2");
        game.UndoLastMove();

        Assert.Equal(before, Evaluate(game));
    }

    [Fact]
    public void ReplacementRookOnOriginalSquareDoesNotRestoreRight()
    {
        var replacement = TestSupport.CreateGame(
            SideDefinitions.Black,
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("g1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.Rook));

        TestSupport.Play(replacement, "h8", "h1");
        TestSupport.Play(replacement, "g1", "h1");

        var apparentlyIntact = TestSupport.CreateGame(
            SideDefinitions.Black,
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));

        Assert.True(
            Evaluate(apparentlyIntact)
                .CastlingRights.WhiteKingSide);
        Assert.False(
            Evaluate(replacement)
                .CastlingRights.WhiteKingSide);
        Assert.NotEqual(CreateKey(apparentlyIntact), CreateKey(replacement));
    }

    [Fact]
    public void LegalEnPassantAvailabilityChangesOtherwiseEqualKey()
    {
        var withEnPassant = CreateImmediateEnPassantOpportunity();
        var withoutEnPassant = RecreateCurrentPosition(withEnPassant);

        var available = Evaluate(withEnPassant);
        var unavailable = Evaluate(withoutEnPassant);

        Assert.Equal(
            TestSupport.Square("d6"),
            available.PositionKey.EffectiveEnPassantTarget);
        Assert.Null(unavailable.PositionKey.EffectiveEnPassantTarget);
        Assert.NotEqual(available.PositionKey, unavailable.PositionKey);
    }

    [Fact]
    public void EnPassantAvailabilityExpiresAfterNextMoveAndUndoRestoresIt()
    {
        var game = CreateImmediateEnPassantOpportunity();
        var before = Evaluate(game);

        TestSupport.Play(game, "h2", "h3");

        Assert.Null(
            Evaluate(game)
                .PositionKey.EffectiveEnPassantTarget);

        game.UndoLastMove();

        Assert.Equal(before, Evaluate(game));
    }

    [Fact]
    public void RawEnPassantTargetComesFromLatestMoveAndUndoRestoresIt()
    {
        var game = Variant.CreateGame();

        TestSupport.Play(game, "e2", "e4");

        var afterDoubleAdvance = Evaluate(game);

        Assert.Equal(
            TestSupport.Square("e3"),
            afterDoubleAdvance.EnPassantTarget);
        Assert.Null(afterDoubleAdvance.EffectiveEnPassantTarget);

        TestSupport.Play(game, "a7", "a6");

        Assert.Null(
            Evaluate(game)
                .EnPassantTarget);

        game.UndoLastMove();

        Assert.Equal(afterDoubleAdvance, Evaluate(game));
    }

    [Fact]
    public void PinnedEnPassantOpportunityDoesNotParticipateInKey()
    {
        var game = TestSupport.CreateGame(
            SideDefinitions.Black,
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e5", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.Rook),
            TestSupport.At("d7", SideDefinitions.Black, PieceDefinitions.Pawn));

        TestSupport.Play(game, "d7", "d5");

        var recreated = RecreateCurrentPosition(game);

        Assert.Null(
            Evaluate(game)
                .PositionKey.EffectiveEnPassantTarget);
        Assert.Equal(CreateKey(game), CreateKey(recreated));
    }

    [Fact]
    public void HalfmoveClockAndReversibleHistoryDoNotParticipateInKey()
    {
        var initial = Variant.CreateGame();
        var repeated = Variant.CreateGame();

        TestSupport.Play(repeated, "g1", "f3");
        TestSupport.Play(repeated, "g8", "f6");
        TestSupport.Play(repeated, "f3", "g1");
        TestSupport.Play(repeated, "f6", "g8");

        Assert.Equal(
            0,
            Evaluate(initial)
                .HalfmoveClock);
        Assert.Equal(
            4,
            Evaluate(repeated)
                .HalfmoveClock);
        Assert.Equal(CreateKey(initial), CreateKey(repeated));
    }

    [Fact]
    public void EvaluateRejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Variant.DefaultPositionFactsEvaluator.Evaluate(null!));
    }

    private static StandardPositionFacts Evaluate(
        Game game)
    {
        return TestSupport
            .CreatePositionFactsEvaluator(game.Variant)
            .Evaluate(game.State);
    }

    private static StandardPositionKey CreateKey(
        Game game)
    {
        return TestSupport
            .CreatePositionFactsEvaluator(game.Variant)
            .CreatePositionKey(game.State);
    }

    private static Game CreatePieceIdentityGame(
        Side side,
        PieceDefinition definition,
        string square)
    {
        return TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At(square, side, definition));
    }

    private static Game CreateWhiteRightsGame()
    {
        return TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));
    }

    private static Game CreateImmediateEnPassantOpportunity()
    {
        var game = Variant.CreateGame();

        TestSupport.Play(game, "e2", "e4");
        TestSupport.Play(game, "a7", "a6");
        TestSupport.Play(game, "e4", "e5");
        TestSupport.Play(game, "d7", "d5");

        return game;
    }

    private static Game RecreateCurrentPosition(
        Game source)
    {
        var currentSide = source.State.CurrentSide;
        var placements = source
            .BoardState
            .Topology
            .Squares
            .Where(square => source.BoardState.TryGetPiece(square, out _))
            .Select(square =>
            {
                source.BoardState.TryGetPiece(square, out var piece);
                return new Placement(square, piece!.Side, piece.Definition);
            })
            .Reverse()
            .ToArray();

        return TestSupport.CreateGame(currentSide, placements);
    }
}
