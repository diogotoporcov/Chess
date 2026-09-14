// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Status;
using Chess.Core.Movement;
using Chess.Core.Sides;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Games.History;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class CustomInitialStateIntegrationTests
{
    [Fact]
    public void MatchingEvaluatorPreservesCustomInitialMetadata()
    {
        var initialState = CreateCounterState(
            SideDefinitions.White,
            halfmoveClock: 87,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var facts = TestSupport
            .CreatePositionFactsEvaluator(initialState)
            .Evaluate(game.State);

        Assert.Equal(NoCastlingRights, facts.CastlingRights);
        Assert.Equal(87, facts.HalfmoveClock);
        Assert.Equal(37, facts.FullmoveNumber);
    }

    [Fact]
    public void FalseCastlingRightIsNotManufacturedByHomeSquarePieces()
    {
        var initialState = CreateCastlingState(hasWhiteKingSide: false);
        var game = Variant.CreateGame(initialState);
        var facts = Evaluate(initialState, game);

        Assert.False(facts.CastlingRights.WhiteKingSide);
        Assert.DoesNotContain(
            new Move(
                TestSupport.Square("e1"),
                TestSupport.Square("g1"),
                MoveOptions.CastleKingSide),
            game.GenerateMoves(TestSupport.Square("e1")));
    }

    [Fact]
    public void TrueCastlingRightEnablesCastlingAndResolverExecution()
    {
        var initialState = CreateCastlingState(hasWhiteKingSide: true);
        var game = Variant.CreateGame(initialState);

        TestSupport.Play(game, "e1", "g1", MoveOptions.CastleKingSide);

        Assert.Same(
            PieceDefinitions.King,
            TestSupport.PieceAt(game, "g1")
                .Definition);
        Assert.Same(
            PieceDefinitions.Rook,
            TestSupport.PieceAt(game, "f1")
                .Definition);
    }

    [Theory]
    [InlineData("king")]
    [InlineData("rook")]
    public void ExplicitCastlingRightIsLostAfterMovementAndUndoRestoresIt(
        string movedPiece)
    {
        var initialState = CreateCastlingState(hasWhiteKingSide: true);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreatePositionFactsEvaluator(initialState);

        if (movedPiece == "king")
        {
            TestSupport.Play(game, "e1", "f1");
            TestSupport.Play(game, "e8", "e7");
            TestSupport.Play(game, "f1", "e1");
        }
        else
        {
            TestSupport.Play(game, "h1", "h2");
            TestSupport.Play(game, "e8", "e7");
            TestSupport.Play(game, "h2", "h1");
        }

        TestSupport.Play(game, "e7", "e8");

        Assert.False(
            evaluator.Evaluate(game.State)
                .CastlingRights.WhiteKingSide);

        for (var index = 0; index < 4; index++)
        {
            game.UndoLastMove();
        }

        Assert.True(
            evaluator.Evaluate(game.State)
                .CastlingRights.WhiteKingSide);
    }

    [Fact]
    public void InitialEnPassantMoveExecutesWithEmptyLocalHistory()
    {
        var initialState = CreateEnPassantState(hasCapturer: true);
        var game = Variant.CreateGame(initialState);
        var whitePawn = TestSupport.PieceAt(game, "e5");
        var blackPawn = TestSupport.PieceAt(game, "d5");

        Assert.Empty(game.State.History);
        Assert.Contains(
            new Move(
                TestSupport.Square("e5"),
                TestSupport.Square("d6"),
                MoveOptions.EnPassant),
            game.GenerateMoves(TestSupport.Square("e5")));

        var record = TestSupport.Play(game, "e5", "d6", MoveOptions.EnPassant);

        Assert.Equal(1, record.PlyNumber);
        Assert.Same(whitePawn, TestSupport.PieceAt(game, "d6"));
        Assert.False(game.BoardState.IsOccupied(TestSupport.Square("d5")));
        Assert.False(game.BoardState.TryGetSquare(blackPawn, out _));
    }

    [Fact]
    public void InitialRawEnPassantExpiresAfterMoveAndReturnsAfterUndo()
    {
        var initialState = CreateEnPassantState(hasCapturer: true);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreatePositionFactsEvaluator(initialState);

        Assert.Equal(
            TestSupport.Square("d6"),
            evaluator.Evaluate(game.State)
                .EnPassantTarget);

        TestSupport.Play(game, "e1", "f1");

        Assert.Null(
            evaluator.Evaluate(game.State)
                .EnPassantTarget);

        game.UndoLastMove();

        Assert.Equal(
            TestSupport.Square("d6"),
            evaluator.Evaluate(game.State)
                .EnPassantTarget);
    }

    [Fact]
    public void RawEnPassantWithoutCapturerIsAcceptedButNotInRepetitionKey()
    {
        var withRawTarget = CreateEnPassantState(hasCapturer: false);
        var withoutTarget = new StandardInitialState(
            withRawTarget.Placements,
            withRawTarget.SideToMove,
            withRawTarget.CastlingRights,
            null,
            0,
            1);
        var withRawGame = Variant.CreateGame(withRawTarget);
        var withoutGame = Variant.CreateGame(withoutTarget);
        var withRawFacts = Evaluate(withRawTarget, withRawGame);
        var withoutFacts = Evaluate(withoutTarget, withoutGame);

        Assert.Equal(TestSupport.Square("d6"), withRawFacts.EnPassantTarget);
        Assert.Null(withRawFacts.EffectiveEnPassantTarget);
        Assert.Null(withoutFacts.EnPassantTarget);
        Assert.Equal(withRawFacts.PositionKey, withoutFacts.PositionKey);
    }

    [Fact]
    public void LegalInitialEnPassantParticipatesInRepetitionKey()
    {
        var withTarget = CreateEnPassantState(hasCapturer: true);
        var withoutTarget = new StandardInitialState(
            withTarget.Placements,
            withTarget.SideToMove,
            withTarget.CastlingRights,
            null,
            0,
            1);
        var withTargetFacts = Evaluate(
            withTarget,
            Variant.CreateGame(withTarget));
        var withoutTargetFacts = Evaluate(
            withoutTarget,
            Variant.CreateGame(withoutTarget));

        Assert.Equal(
            TestSupport.Square("d6"),
            withTargetFacts.EffectiveEnPassantTarget);
        Assert.Null(withoutTargetFacts.EffectiveEnPassantTarget);
        Assert.NotEqual(
            withTargetFacts.PositionKey,
            withoutTargetFacts.PositionKey);
    }

    [Fact]
    public void LoadedHalfmoveClockReachesClaimThresholdWithoutOutcome()
    {
        var initialState = CreateCounterState(
            SideDefinitions.White,
            halfmoveClock: 99,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreatePositionFactsEvaluator(initialState);

        TestSupport.Play(game, "a1", "a2");

        Assert.Equal(
            100,
            evaluator.Evaluate(game.State)
                .HalfmoveClock);
        Assert.True(
            new StandardHalfmoveRuleFacts(100).IsFiftyMoveThresholdReached);
        Assert.Null(game.Outcome);
    }

    [Fact]
    public void LoadedHalfmoveClockReachesAutomaticDrawThreshold()
    {
        var initialState = CreateCounterState(
            SideDefinitions.White,
            halfmoveClock: 149,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreatePositionFactsEvaluator(initialState);

        TestSupport.Play(game, "a1", "a2");

        var outcome = Assert.IsType<GameOutcome>(game.Outcome);

        Assert.Equal(
            150,
            evaluator.Evaluate(game.State)
                .HalfmoveClock);
        Assert.Equal(
            TerminationDefinitions.SeventyFiveMoveRule,
            outcome.Termination);
    }

    [Fact]
    public void LoadedPositionAtAutomaticDrawThresholdHasOutcomeImmediately()
    {
        var initialState = CreateCounterState(
            SideDefinitions.White,
            halfmoveClock: 150,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var outcome = Assert.IsType<GameOutcome>(game.Outcome);

        Assert.Equal(
            TerminationDefinitions.SeventyFiveMoveRule,
            outcome.Termination);
    }

    [Fact]
    public void PawnMoveAndCaptureResetLoadedHalfmoveClockAndUndoRestoresIt()
    {
        var pawnMoveState = TestSupport.CreateInitialState(
            SideDefinitions.White,
            [
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "e2",
                    SideDefinitions.White,
                    PieceDefinitions.Pawn)
            ],
            castlingRights: NoCastlingRights,
            halfmoveClock: 87);
        var pawnMoveGame = Variant.CreateGame(pawnMoveState);
        var pawnMoveEvaluator =
            TestSupport.CreatePositionFactsEvaluator(pawnMoveState);

        TestSupport.Play(pawnMoveGame, "e2", "e3");
        Assert.Equal(
            0,
            pawnMoveEvaluator.Evaluate(pawnMoveGame.State)
                .HalfmoveClock);
        pawnMoveGame.UndoLastMove();
        Assert.Equal(
            87,
            pawnMoveEvaluator.Evaluate(pawnMoveGame.State)
                .HalfmoveClock);

        var captureState = TestSupport.CreateInitialState(
            SideDefinitions.White,
            [
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "e4",
                    SideDefinitions.White,
                    PieceDefinitions.Pawn),
                TestSupport.At(
                    "d5",
                    SideDefinitions.Black,
                    PieceDefinitions.Knight)
            ],
            castlingRights: NoCastlingRights,
            halfmoveClock: 87);
        var captureGame = Variant.CreateGame(captureState);
        var captureEvaluator =
            TestSupport.CreatePositionFactsEvaluator(captureState);

        TestSupport.Play(captureGame, "e4", "d5");
        Assert.Equal(
            0,
            captureEvaluator.Evaluate(captureGame.State)
                .HalfmoveClock);
        captureGame.UndoLastMove();
        Assert.Equal(
            87,
            captureEvaluator.Evaluate(captureGame.State)
                .HalfmoveClock);
    }

    [Fact]
    public void WhiteToMoveFullmoveNumberChangesOnlyAfterBlackAndUndoRestores()
    {
        var initialState = CreateCounterState(
            SideDefinitions.White,
            halfmoveClock: 0,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreatePositionFactsEvaluator(initialState);

        Assert.Equal(
            37,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);

        var whiteRecord = TestSupport.Play(game, "a1", "a2");
        Assert.Equal(1, whiteRecord.PlyNumber);
        Assert.Equal(
            37,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);

        TestSupport.Play(game, "a8", "a7");
        Assert.Equal(
            38,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);

        game.UndoLastMove();
        Assert.Equal(
            37,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);
        game.UndoLastMove();
        Assert.Equal(
            37,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);
    }

    [Fact]
    public void BlackToMoveFullmoveNumberChangesAfterFirstLocalMoveAndUndo()
    {
        var initialState = CreateCounterState(
            SideDefinitions.Black,
            halfmoveClock: 0,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreatePositionFactsEvaluator(initialState);

        Assert.Equal(
            [SideDefinitions.White, SideDefinitions.Black],
            game.State.TurnOrder.Sides);
        Assert.Equal(
            37,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);

        var record = TestSupport.Play(game, "a8", "a7");

        Assert.Equal(1, record.PlyNumber);
        Assert.Equal(
            38,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);

        game.UndoLastMove();

        Assert.Equal(
            37,
            evaluator.Evaluate(game.State)
                .FullmoveNumber);
        Assert.Same(SideDefinitions.Black, game.State.CurrentSide);
    }

    [Fact]
    public void ArbitraryStartRepetitionTracksCycleAndUndo()
    {
        var initialState = CreateCounterState(
            SideDefinitions.White,
            halfmoveClock: 12,
            fullmoveNumber: 37);
        var game = Variant.CreateGame(initialState);
        var evaluator = TestSupport.CreateRepetitionEvaluator(initialState);
        var factsEvaluator =
            TestSupport.CreatePositionFactsEvaluator(initialState);
        var initialKey = factsEvaluator.CreatePositionKey(game.State);

        Assert.Equal(
            1,
            evaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);

        TestSupport.Play(game, "a1", "a2");
        TestSupport.Play(game, "a8", "a7");
        TestSupport.Play(game, "a2", "a1");
        TestSupport.Play(game, "a7", "a8");

        Assert.Equal(initialKey, factsEvaluator.CreatePositionKey(game.State));
        Assert.Equal(
            2,
            evaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);

        for (var index = 0; index < 4; index++)
        {
            game.UndoLastMove();
        }

        Assert.Equal(
            1,
            evaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);
    }

    private static StandardPositionFacts Evaluate(
        StandardInitialState initialState,
        Game game)
    {
        return TestSupport
            .CreatePositionFactsEvaluator(initialState)
            .Evaluate(game.State);
    }

    private static StandardInitialState CreateCastlingState(
        bool hasWhiteKingSide)
    {
        return TestSupport.CreateInitialState(
            SideDefinitions.White,
            [
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "h1",
                    SideDefinitions.White,
                    PieceDefinitions.Rook),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King)
            ],
            new CastlingRights(hasWhiteKingSide, false, false, false));
    }

    private static StandardInitialState CreateEnPassantState(
        bool hasCapturer)
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
                "d5",
                SideDefinitions.Black,
                PieceDefinitions.Pawn)
        };

        if (hasCapturer)
        {
            placements.Add(
                TestSupport.At(
                    "e5",
                    SideDefinitions.White,
                    PieceDefinitions.Pawn));
        }

        return TestSupport.CreateInitialState(
            SideDefinitions.White,
            placements,
            castlingRights: NoCastlingRights,
            enPassantTarget: TestSupport.Square("d6"));
    }

    private static StandardInitialState CreateCounterState(
        Side sideToMove,
        int halfmoveClock,
        int fullmoveNumber)
    {
        return TestSupport.CreateInitialState(
            sideToMove,
            [
                TestSupport.At(
                    "e1",
                    SideDefinitions.White,
                    PieceDefinitions.King),
                TestSupport.At(
                    "a1",
                    SideDefinitions.White,
                    PieceDefinitions.Rook),
                TestSupport.At(
                    "e8",
                    SideDefinitions.Black,
                    PieceDefinitions.King),
                TestSupport.At(
                    "a8",
                    SideDefinitions.Black,
                    PieceDefinitions.Rook)
            ],
            castlingRights: NoCastlingRights,
            halfmoveClock: halfmoveClock,
            fullmoveNumber: fullmoveNumber);
    }

    private static CastlingRights NoCastlingRights =>
        new(false, false, false, false);
}
