// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Attacks;
using Chess.Core.Games.Status;
using Chess.Core.Games.Variants;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Games.History;
using Chess.Variants.Standard.Games.Rules;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class AutomaticDrawStatusTests
{
    [Fact]
    public void FivefoldIsAutomaticWhileThirdAndFourthOccurrencesAreNot()
    {
        var game = Variant.CreateGame();

        PlayInitialPositionCycle(game);
        PlayInitialPositionCycle(game);

        Assert.Equal(
            3,
            RepetitionFacts(game)
                .CurrentPositionOccurrences);
        Assert.Equal(StatusDefinitions.Active, game.Status.Id);
        Assert.False(game.Status.IsTerminal);
        Assert.Null(game.Status.Outcome);

        PlayInitialPositionCycle(game);

        Assert.Equal(
            4,
            RepetitionFacts(game)
                .CurrentPositionOccurrences);
        Assert.Equal(StatusDefinitions.Active, game.Status.Id);
        Assert.False(game.Status.IsTerminal);
        Assert.Null(game.Status.Outcome);

        PlayInitialPositionCycle(game);

        var status = game.Status;
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(
            5,
            RepetitionFacts(game)
                .CurrentPositionOccurrences);
        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.Equal(
            TerminationDefinitions.FivefoldRepetition,
            outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);

        var otherwiseLegalMove = TestSupport.FindMove(game, "g1", "f3");

        Assert.Throws<InvalidOperationException>(() =>
            game.Execute(otherwiseLegalMove));

        game.UndoLastMove();

        Assert.False(game.Status.IsTerminal);
        Assert.Null(game.Status.Outcome);
    }

    [Fact]
    public void FiftyMovesRemainNonTerminalAndSeventyFiveMovesAreAutomatic()
    {
        var placements = CreatePeriodicHistoryPlacements();
        var game = TestSupport.CreateGame(placements);
        var repetitionEvaluator = CreateRepetitionEvaluator(game.Variant);

        PlayPeriodicQuietMoves(game, 60);

        Assert.Equal(
            2,
            repetitionEvaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);

        PlayPeriodicQuietMoves(game, 100);

        Assert.Equal(
            100,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.True(
            HalfmoveFacts(game)
                .IsFiftyMoveThresholdReached);
        Assert.False(game.Status.IsTerminal);
        Assert.Null(game.Status.Outcome);

        PlayPeriodicQuietMoves(game, 120);

        Assert.Equal(
            3,
            repetitionEvaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);

        PlayPeriodicQuietMoves(game, 149);

        Assert.Equal(
            149,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.False(
            HalfmoveFacts(game)
                .IsSeventyFiveMoveThresholdReached);
        Assert.False(game.Status.IsTerminal);
        Assert.Null(game.Status.Outcome);

        PlayPeriodicQuietMoves(game, 150);

        var status = game.Status;
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(
            150,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.True(
            HalfmoveFacts(game)
                .IsSeventyFiveMoveThresholdReached);
        Assert.False(
            repetitionEvaluator.Evaluate(game.State)
                .IsFivefoldRepetition);
        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.Equal(
            TerminationDefinitions.SeventyFiveMoveRule,
            outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);

        var otherwiseLegalMove = TestSupport
            .AllMoves(game)[0];

        Assert.Throws<InvalidOperationException>(() =>
            game.Execute(otherwiseLegalMove));

        game.UndoLastMove();

        Assert.Equal(
            149,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.False(game.Status.IsTerminal);
        Assert.Null(game.Status.Outcome);
    }

    [Fact]
    public void SeventyFiveMoveRulePrecedesFivefoldWhenBothApply()
    {
        var game = TestSupport.CreateNonTerminatingGame(
            CreateLongHistoryPlacements());

        for (var cycle = 0; cycle < 37; cycle++)
        {
            PlayInitialPositionCycle(game);
        }

        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "g8", "f6");

        var status = TestSupport
            .CreateStatusEvaluator(game.Variant)
            .Evaluate(game.State);
        var repetitionFacts = CreateRepetitionEvaluator(game.Variant)
            .Evaluate(game.State);
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.True(repetitionFacts.IsFivefoldRepetition);
        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.Equal(
            TerminationDefinitions.SeventyFiveMoveRule,
            outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
    }

    [Fact]
    public void DeadPositionPrecedesHistoricalAutomaticDraws()
    {
        var game = TestSupport.CreateNonTerminatingGame(
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At(
                "c1",
                SideDefinitions.White,
                PieceDefinitions.Bishop),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.King));

        for (var cycle = 0; cycle < 37; cycle++)
        {
            TestSupport.Play(game, "a1", "a2");
            TestSupport.Play(game, "h8", "h7");
            TestSupport.Play(game, "a2", "a1");
            TestSupport.Play(game, "h7", "h8");
        }

        TestSupport.Play(game, "a1", "a2");
        TestSupport.Play(game, "h8", "h7");

        var status = TestSupport
            .CreateStatusEvaluator(game.Variant)
            .Evaluate(game.State);
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(
            150,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.True(
            RepetitionFacts(game)
                .IsFivefoldRepetition);
        Assert.True(
            InsufficientMatingMaterialDetector.IsInsufficient(game.State));
        Assert.Equal(StatusDefinitions.Active, status.Id);
        Assert.Equal(TerminationDefinitions.DeadPosition, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
    }

    [Fact]
    public void SeventyFiveMoveOutcomePreservesCheckStatus()
    {
        var game = CreateCheckAtThresholdGame();

        for (var cycle = 0; cycle < 37; cycle++)
        {
            PlayBlackFirstKnightCycle(game);
        }

        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "b1", "b5");

        var checkDetector = new CheckDetector(new PatternAttackGenerator());
        var status = TestSupport
            .CreateStatusEvaluator(game.Variant)
            .Evaluate(game.State);
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.True(checkDetector.IsInCheck(game.State, SideDefinitions.Black));
        Assert.NotEmpty(TestSupport.AllMoves(game));
        Assert.Equal(StatusDefinitions.Check, status.Id);
        Assert.Equal(
            TerminationDefinitions.SeventyFiveMoveRule,
            outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
    }

    [Fact]
    public void CheckmatePrecedesAutomaticDrawsAtHalfmove150()
    {
        var game = CreatePositionTerminationGame();

        for (var cycle = 0; cycle < 37; cycle++)
        {
            PlayBlackFirstKnightCycle(game);
        }

        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "c7", "b7");

        var status = TestSupport
            .CreateStatusEvaluator(game.Variant)
            .Evaluate(game.State);
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(
            150,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.Equal(StatusDefinitions.Checkmate, status.Id);
        Assert.Equal(TerminationDefinitions.Checkmate, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Equal([SideDefinitions.White], outcome.Winners);
    }

    [Fact]
    public void StalematePrecedesAutomaticDrawsAtHalfmove150()
    {
        var game = CreateStalemateTerminationGame();

        for (var cycle = 0; cycle < 37; cycle++)
        {
            PlayBlackKingCycle(game);
        }

        TestSupport.Play(game, "b8", "a8");
        TestSupport.Play(game, "e3", "b6");

        var status = TestSupport
            .CreateStatusEvaluator(game.Variant)
            .Evaluate(game.State);
        var outcome = Assert.IsType<GameOutcome>(status.Outcome);

        Assert.Equal(
            150,
            HalfmoveFacts(game)
                .HalfmoveClock);
        Assert.Equal(StatusDefinitions.Stalemate, status.Id);
        Assert.Equal(TerminationDefinitions.Stalemate, outcome.Termination);
        Assert.True(status.IsTerminal);
        Assert.Empty(outcome.Winners);
    }

    [Fact]
    public void StatusEvaluatorRejectsNullDependencies()
    {
        var definition = TestSupport.CreateDefinition(
            new TurnOrder(SideDefinitions.White, SideDefinitions.Black),
            CreateLongHistoryPlacements());
        var executionResolver = TestSupport.CreateExecutionResolver();
        var legalMoveGenerator =
            TestSupport.CreateLegalMoveGenerator(executionResolver);
        var checkDetector = new CheckDetector(new PatternAttackGenerator());
        var moveResolver = new GameMoveResolver(
            legalMoveGenerator,
            executionResolver);
        var positionFactsEvaluator = new StandardPositionFactsEvaluator(
            legalMoveGenerator);
        var repetitionEvaluator = new StandardRepetitionEvaluator(
            positionFactsEvaluator,
            TestSupport.CreateGameStateFactory(definition)
                .Create,
            new GameMoveExecutor(moveResolver));
        var halfmoveRuleEvaluator = new StandardHalfmoveRuleEvaluator(
            positionFactsEvaluator,
            moveResolver);

        Assert.Throws<ArgumentNullException>(() => new StatusEvaluator(
            null!,
            checkDetector,
            repetitionEvaluator,
            halfmoveRuleEvaluator));
        Assert.Throws<ArgumentNullException>(() => new StatusEvaluator(
            legalMoveGenerator,
            null!,
            repetitionEvaluator,
            halfmoveRuleEvaluator));
        Assert.Throws<ArgumentNullException>(() => new StatusEvaluator(
            legalMoveGenerator,
            checkDetector,
            null!,
            halfmoveRuleEvaluator));
        Assert.Throws<ArgumentNullException>(() => new StatusEvaluator(
            legalMoveGenerator,
            checkDetector,
            repetitionEvaluator,
            null!));
    }

    private static void PlayPeriodicQuietMoves(
        Game game,
        int targetHalfmoveCount)
    {
        string[] whiteKingCycle = ["a1", "b1", "c1", "c2", "b2"];
        string[] blackKingCycle = ["h8", "g8", "f8", "f7", "g7", "h7"];

        while (game.State.History.Count < targetHalfmoveCount)
        {
            var ply = game.State.History.Count;
            var cycle = ply % 2 == 0 ? whiteKingCycle : blackKingCycle;

            PlayCycleMove(game, cycle, ply / 2);
        }
    }

    private static void PlayCycleMove(
        Game game,
        string[] cycle,
        int moveNumber)
    {
        var fromIndex = moveNumber % cycle.Length;
        var toIndex = (fromIndex + 1) % cycle.Length;

        TestSupport.Play(game, cycle[fromIndex], cycle[toIndex]);
    }

    private static Placement[] CreatePeriodicHistoryPlacements()
    {
        return
        [
            TestSupport.At(
                "a1",
                SideDefinitions.White,
                PieceDefinitions.King),
            TestSupport.At(
                "h1",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At(
                "h8",
                SideDefinitions.Black,
                PieceDefinitions.King),
            TestSupport.At(
                "a8",
                SideDefinitions.Black,
                PieceDefinitions.Knight)
        ];
    }

    private static Placement[] CreateLongHistoryPlacements()
    {
        return
        [
            TestSupport.At(
                "a1",
                SideDefinitions.White,
                PieceDefinitions.King),
            TestSupport.At(
                "b1",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At(
                "g1",
                SideDefinitions.White,
                PieceDefinitions.Knight),
            TestSupport.At(
                "h8",
                SideDefinitions.Black,
                PieceDefinitions.King),
            TestSupport.At(
                "b8",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At(
                "g8",
                SideDefinitions.Black,
                PieceDefinitions.Knight)
        ];
    }

    private static Game CreatePositionTerminationGame()
    {
        return TestSupport.CreateNonTerminatingGame(
            new TurnOrder(SideDefinitions.Black, SideDefinitions.White),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At(
                "g8",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At("c6", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("c7", SideDefinitions.White, PieceDefinitions.Queen),
            TestSupport.At(
                "g1",
                SideDefinitions.White,
                PieceDefinitions.Knight));
    }

    private static Game CreateStalemateTerminationGame()
    {
        return TestSupport.CreateNonTerminatingGame(
            new TurnOrder(SideDefinitions.Black, SideDefinitions.White),
            TestSupport.At("b8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("c6", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e3", SideDefinitions.White, PieceDefinitions.Queen),
            TestSupport.At(
                "g1",
                SideDefinitions.White,
                PieceDefinitions.Knight));
    }

    private static Game CreateCheckAtThresholdGame()
    {
        return TestSupport.CreateNonTerminatingGame(
            new TurnOrder(SideDefinitions.Black, SideDefinitions.White),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At(
                "g8",
                SideDefinitions.Black,
                PieceDefinitions.Knight),
            TestSupport.At("a1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("b1", SideDefinitions.White, PieceDefinitions.Queen),
            TestSupport.At(
                "g1",
                SideDefinitions.White,
                PieceDefinitions.Knight));
    }

    private static StandardRepetitionEvaluator CreateRepetitionEvaluator(
        GameVariantDefinition definition)
    {
        var executionResolver = TestSupport.CreateExecutionResolver();
        var legalMoveGenerator =
            TestSupport.CreateLegalMoveGenerator(executionResolver);
        var moveResolver = new GameMoveResolver(
            legalMoveGenerator,
            executionResolver);

        return new StandardRepetitionEvaluator(
            new StandardPositionFactsEvaluator(legalMoveGenerator),
            TestSupport.CreateGameStateFactory(definition)
                .Create,
            new GameMoveExecutor(moveResolver));
    }

    private static StandardRepetitionFacts RepetitionFacts(
        Game game)
    {
        return CreateRepetitionEvaluator(game.Variant)
            .Evaluate(game.State);
    }

    private static StandardHalfmoveRuleFacts HalfmoveFacts(
        Game game)
    {
        return Variant.HalfmoveRuleEvaluator.Evaluate(game.State);
    }

    private static void PlayInitialPositionCycle(
        Game game)
    {
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "f3", "g1");
        TestSupport.Play(game, "f6", "g8");
    }

    private static void PlayBlackFirstKnightCycle(
        Game game)
    {
        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "f6", "g8");
        TestSupport.Play(game, "f3", "g1");
    }

    private static void PlayBlackKingCycle(
        Game game)
    {
        TestSupport.Play(game, "b8", "a8");
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "a8", "b8");
        TestSupport.Play(game, "f3", "g1");
    }
}
