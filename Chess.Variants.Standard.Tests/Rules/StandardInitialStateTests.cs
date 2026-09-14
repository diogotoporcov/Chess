// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games.Variants;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class StandardInitialStateTests
{
    [Fact]
    public void PlacementsAreDefensivelyCopiedAndExposedReadOnly()
    {
        var source = CreateKings();
        var state = Create(source);
        var originalCount = state.Placements.Count;

        source.Clear();

        Assert.Equal(originalCount, state.Placements.Count);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<InitialPiecePlacement>)state.Placements).Clear());
    }

    [Fact]
    public void PlacementsRejectNullCollectionAndElement()
    {
        Assert.Throws<ArgumentNullException>(() => new StandardInitialState(
            null!,
            SideDefinitions.White,
            default,
            null,
            0,
            1));
        Assert.Throws<ArgumentException>(() =>
            Create([.. CreateKings(), null!]));
    }

    [Fact]
    public void PlacementsRejectDuplicateAndOutsideSquares()
    {
        var duplicate = CreateKings();
        duplicate.Add(
            new InitialPiecePlacement(
                TestSupport.Square("e1"),
                SideDefinitions.White,
                PieceDefinitions.Queen));
        var outside = CreateKings();
        outside.Add(
            new InitialPiecePlacement(
                new Square(64),
                SideDefinitions.White,
                PieceDefinitions.Queen));

        Assert.Throws<ArgumentException>(() => Create(duplicate));
        Assert.Throws<ArgumentException>(() => Create(outside));
    }

    [Fact]
    public void PlacementsRequireStandardSidesAndCanonicalPieceDefinitions()
    {
        var unsupportedSide = CreateKings();
        unsupportedSide.Add(
            new InitialPiecePlacement(
                TestSupport.Square("a2"),
                new Side("test:outsider"),
                PieceDefinitions.Pawn));
        var unsupportedDefinition = CreateKings();
        unsupportedDefinition.Add(
            new InitialPiecePlacement(
                TestSupport.Square("a2"),
                SideDefinitions.White,
                new PieceDefinition(
                    PieceDefinitions.Pawn.Id,
                    PieceDefinitions.Pawn.Name)));

        Assert.Throws<ArgumentException>(() => Create(unsupportedSide));
        Assert.Throws<ArgumentException>(() => Create(unsupportedDefinition));
    }

    [Fact]
    public void EquivalentSideIdsAreNormalizedToCanonicalSides()
    {
        var placements = new[]
        {
            new InitialPiecePlacement(
                TestSupport.Square("e1"),
                new Side("chess:white"),
                PieceDefinitions.King),
            new InitialPiecePlacement(
                TestSupport.Square("e8"),
                new Side("chess:black"),
                PieceDefinitions.King)
        };

        var state = new StandardInitialState(
            placements,
            new Side("chess:black"),
            default,
            null,
            0,
            1);

        Assert.Same(SideDefinitions.Black, state.SideToMove);
        Assert.Same(SideDefinitions.White, state.Placements[0].Side);
        Assert.Same(SideDefinitions.Black, state.Placements[1].Side);
    }

    [Fact]
    public void PlacementsRequireExactlyOneKingPerSide()
    {
        var missingWhite = CreateKings();
        missingWhite.RemoveAt(0);
        var missingBlack = CreateKings();
        missingBlack.RemoveAt(1);
        var multipleWhite = CreateKings();
        multipleWhite.Add(
            new InitialPiecePlacement(
                TestSupport.Square("a1"),
                SideDefinitions.White,
                PieceDefinitions.King));

        Assert.Throws<ArgumentException>(() => Create(missingWhite));
        Assert.Throws<ArgumentException>(() => Create(missingBlack));
        Assert.Throws<ArgumentException>(() => Create(multipleWhite));
    }

    [Theory]
    [InlineData("a1")]
    [InlineData("a8")]
    public void PawnsCannotOccupyFirstOrEighthRank(
        string square)
    {
        var placements = CreateKings();
        placements.Add(
            new InitialPiecePlacement(
                TestSupport.Square(square),
                SideDefinitions.White,
                PieceDefinitions.Pawn));

        Assert.Throws<ArgumentException>(() => Create(placements));
    }

    [Fact]
    public void SideMaterialLimitsAreValidated()
    {
        var tooManyPieces = CreateKings();

        foreach (var coordinate in new[]
                 {
                     "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2", "a3",
                     "b3", "c3", "d3", "e3", "f3", "g3", "h3"
                 })
        {
            tooManyPieces.Add(
                new InitialPiecePlacement(
                    TestSupport.Square(coordinate),
                    SideDefinitions.White,
                    PieceDefinitions.Knight));
        }

        var tooManyPawns = CreateKings();

        foreach (var coordinate in new[]
                 {
                     "a2", "b2", "c2", "d2", "e2", "f2", "g2", "h2", "a3"
                 })
        {
            tooManyPawns.Add(
                new InitialPiecePlacement(
                    TestSupport.Square(coordinate),
                    SideDefinitions.White,
                    PieceDefinitions.Pawn));
        }

        Assert.Throws<ArgumentException>(() => Create(tooManyPieces));
        Assert.Throws<ArgumentException>(() => Create(tooManyPawns));
    }

    [Fact]
    public void SideToMoveAndCountersAreValidated()
    {
        Assert.Throws<ArgumentNullException>(() => Variant.CreateGame(null!));
        Assert.Throws<ArgumentException>(() => new StandardInitialState(
            CreateKings(),
            new Side("test:outsider"),
            default,
            null,
            0,
            1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StandardInitialState(
                CreateKings(),
                SideDefinitions.White,
                default,
                null,
                -1,
                1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StandardInitialState(
                CreateKings(),
                SideDefinitions.White,
                default,
                null,
                0,
                0));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StandardInitialState(
                CreateKings(),
                SideDefinitions.White,
                default,
                null,
                0,
                -1));
    }

    [Fact]
    public void CastlingRightsRequireKingAndRookOnHomeSquares()
    {
        var kingElsewhere = CreateKings("d1");
        kingElsewhere.Add(
            new InitialPiecePlacement(
                TestSupport.Square("h1"),
                SideDefinitions.White,
                PieceDefinitions.Rook));
        var rookMissing = CreateKings();

        Assert.Throws<ArgumentException>(() => new StandardInitialState(
            kingElsewhere,
            SideDefinitions.White,
            new CastlingRights(true, false, false, false),
            null,
            0,
            1));
        Assert.Throws<ArgumentException>(() => new StandardInitialState(
            rookMissing,
            SideDefinitions.White,
            new CastlingRights(true, false, false, false),
            null,
            0,
            1));
    }

    [Fact]
    public void RawEnPassantTargetRequiresDoublePawnStructure()
    {
        var missingPawn = CreateKings();
        var occupiedOrigin = CreateKings();
        occupiedOrigin.Add(Placement("d5", SideDefinitions.Black));
        occupiedOrigin.Add(Placement("d7", SideDefinitions.Black));
        var occupiedTarget = CreateKings();
        occupiedTarget.Add(Placement("d5", SideDefinitions.Black));
        occupiedTarget.Add(Placement("d6", SideDefinitions.White));

        Assert.Throws<ArgumentException>(() => Create(
            missingPawn,
            enPassantTarget: TestSupport.Square("d6")));
        Assert.Throws<ArgumentException>(() => Create(
            occupiedOrigin,
            enPassantTarget: TestSupport.Square("d6")));
        Assert.Throws<ArgumentException>(() => Create(
            occupiedTarget,
            enPassantTarget: TestSupport.Square("d6")));
        Assert.Throws<ArgumentException>(() => Create(
            [.. CreateKings(), Placement("d5", SideDefinitions.Black)],
            enPassantTarget: TestSupport.Square("d4")));
        Assert.Throws<ArgumentException>(() => Create(
            [.. CreateKings(), Placement("d5", SideDefinitions.Black)],
            enPassantTarget: TestSupport.Square("d6"),
            halfmoveClock: 1));
    }

    [Fact]
    public void BlackToMoveRawEnPassantTargetAcceptsWhiteDoubleAdvance()
    {
        var state = Create(
            [.. CreateKings(), Placement("d4", SideDefinitions.White)],
            SideDefinitions.Black,
            TestSupport.Square("d3"));

        Assert.Equal(TestSupport.Square("d3"), state.EnPassantTarget);
    }

    private static StandardInitialState Create(
        IEnumerable<InitialPiecePlacement> placements,
        Side? sideToMove = null,
        Square? enPassantTarget = null,
        int halfmoveClock = 0)
    {
        return new StandardInitialState(
            placements,
            sideToMove ?? SideDefinitions.White,
            default,
            enPassantTarget,
            halfmoveClock,
            1);
    }

    private static List<InitialPiecePlacement> CreateKings(
        string white = "e1",
        string black = "e8")
    {
        return
        [
            new InitialPiecePlacement(
                TestSupport.Square(white),
                SideDefinitions.White,
                PieceDefinitions.King),
            new InitialPiecePlacement(
                TestSupport.Square(black),
                SideDefinitions.Black,
                PieceDefinitions.King)
        ];
    }

    private static InitialPiecePlacement Placement(
        string coordinate,
        Side side)
    {
        return new InitialPiecePlacement(
            TestSupport.Square(coordinate),
            side,
            PieceDefinitions.Pawn);
    }
}
