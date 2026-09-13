// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;
using Chess.Variants.Standard.Games.History;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class StandardRepetitionEvaluatorTests
{
    [Fact]
    public void InitialPositionHasOneOccurrence()
    {
        var game = Variant.CreateGame();
        var facts = Evaluate(game);

        Assert.Equal(1, facts.CurrentPositionOccurrences);
        Assert.False(facts.IsThreefoldRepetition);
        Assert.False(facts.IsFivefoldRepetition);
    }

    [Fact]
    public void CurrentRepetitionEvaluationDoesNotMutateSourceGame()
    {
        var game = Variant.CreateGame();

        PlayInitialPositionCycle(game);

        var snapshot = StandardGameSnapshot.Capture(game);

        _ = Evaluate(game);

        snapshot.AssertMatches(game);
    }

    [Fact]
    public void ReversibleCyclesCountSecondThirdAndFifthOccurrences()
    {
        var game = Variant.CreateGame();

        PlayInitialPositionCycle(game);
        var secondOccurrence = Evaluate(game);

        Assert.Equal(2, secondOccurrence.CurrentPositionOccurrences);

        PlayInitialPositionCycle(game);
        var thirdOccurrence = Evaluate(game);

        Assert.Equal(3, thirdOccurrence.CurrentPositionOccurrences);
        Assert.True(thirdOccurrence.IsThreefoldRepetition);
        Assert.False(thirdOccurrence.IsFivefoldRepetition);

        PlayInitialPositionCycle(game);
        PlayInitialPositionCycle(game);
        var fifthOccurrence = Evaluate(game);

        Assert.Equal(5, fifthOccurrence.CurrentPositionOccurrences);
        Assert.True(fifthOccurrence.IsThreefoldRepetition);
        Assert.True(fifthOccurrence.IsFivefoldRepetition);
    }

    [Fact]
    public void NonConsecutiveReturnsToInitialPositionAreCounted()
    {
        var game = Variant.CreateGame();

        PlayInitialPositionCycle(game);
        TestSupport.Play(game, "b1", "c3");
        TestSupport.Play(game, "b8", "c6");
        TestSupport.Play(game, "c3", "b1");
        TestSupport.Play(game, "c6", "b8");

        Assert.Equal(
            3,
            Evaluate(game)
                .CurrentPositionOccurrences);
    }

    [Fact]
    public void ReturnedPieceLayoutWithLostCastlingRightsIsNotRepetition()
    {
        var definition = TestSupport.CreateDefinition(
            new TurnOrder(SideDefinitions.White, SideDefinitions.Black),
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.Rook),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));
        var game = definition.CreateGame();
        var evaluator = CreateEvaluator(definition);

        TestSupport.Play(game, "h1", "h2");
        TestSupport.Play(game, "e8", "e7");
        TestSupport.Play(game, "h2", "h1");
        TestSupport.Play(game, "e7", "e8");

        Assert.Equal(
            1,
            evaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);
    }

    [Fact]
    public void LegalEnPassantTargetParticipatesInReconstructedPositionKey()
    {
        var game = Variant.CreateGame();

        TestSupport.Play(game, "e2", "e4");
        TestSupport.Play(game, "a7", "a6");
        TestSupport.Play(game, "e4", "e5");
        TestSupport.Play(game, "d7", "d5");

        Assert.Equal(
            1,
            Evaluate(game)
                .CurrentPositionOccurrences);
        Assert.Equal(
            TestSupport.Square("d6"),
            Variant.PositionFactsEvaluator.CreatePositionKey(game.State)
                .EffectiveEnPassantTarget);
    }

    [Fact]
    public void PinnedEnPassantDoesNotParticipateInReconstructedPositionKey()
    {
        var definition = TestSupport.CreateDefinition(
            new TurnOrder(SideDefinitions.Black, SideDefinitions.White),
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e5", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("a8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.Rook),
            TestSupport.At("d7", SideDefinitions.Black, PieceDefinitions.Pawn));
        var game = definition.CreateGame();
        var evaluator = CreateEvaluator(definition);

        TestSupport.Play(game, "d7", "d5");

        Assert.Equal(
            1,
            evaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);
        Assert.Null(
            Variant.PositionFactsEvaluator.CreatePositionKey(game.State)
                .EffectiveEnPassantTarget);
    }

    [Fact]
    public void ReplayPreservesPromotionMoveOption()
    {
        var definition = TestSupport.CreateDefinition(
            new TurnOrder(SideDefinitions.White, SideDefinitions.Black),
            TestSupport.At("h1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("h8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("a7", SideDefinitions.White, PieceDefinitions.Pawn));
        var game = definition.CreateGame();
        var evaluator = CreateEvaluator(definition);

        TestSupport.Play(game, "a7", "a8", PromotionOptions.Queen);

        Assert.Equal(
            1,
            evaluator.Evaluate(game.State)
                .CurrentPositionOccurrences);
        Assert.Equal(
            PieceDefinitions.Queen,
            TestSupport.PieceAt(game, "a8")
                .Definition);
        Assert.Contains(
            Variant.PositionFactsEvaluator.CreatePositionKey(game.State)
                .PiecePlacements,
            placement => placement.Square == TestSupport.Square("a8") &&
                         placement.PieceDefinitionId ==
                         PieceDefinitions.Queen.Id.Value);
    }

    [Fact]
    public void ReconstructedStateMustMatchSuppliedState()
    {
        var definition = TestSupport.CreateDefinition(
            new TurnOrder(SideDefinitions.White, SideDefinitions.Black),
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King));
        var game = definition.CreateGame();
        var snapshot = StandardGameSnapshot.Capture(game);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            Variant.RepetitionEvaluator.Evaluate(game.State));

        Assert.Contains("cannot be reconstructed", exception.Message);
        snapshot.AssertMatches(game);
    }

    [Fact]
    public void
        LegalMoveThatWouldCreateThirdOccurrenceReturnsTrueWithoutMutation()
    {
        var game = Variant.CreateGame();

        PlayInitialPositionCycle(game);
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "f3", "g1");

        var snapshot = StandardGameSnapshot.Capture(game);
        var createsThird = TestSupport.FindMove(game, "f6", "g8");
        var doesNotCreateThird = TestSupport.FindMove(game, "b8", "c6");

        Assert.True(
            Variant.RepetitionEvaluator.WouldCreateThreefoldRepetition(
                game.State,
                createsThird));
        Assert.False(
            Variant.RepetitionEvaluator.WouldCreateThreefoldRepetition(
                game.State,
                doesNotCreateThird));
        snapshot.AssertMatches(game);
    }

    [Fact]
    public void IllegalMoveThrowsWithoutMutatingSourceGame()
    {
        var game = Variant.CreateGame();
        var snapshot = StandardGameSnapshot.Capture(game);
        var illegalMove = new Move(
            TestSupport.Square("e2"),
            TestSupport.Square("e5"));

        Assert.Throws<InvalidOperationException>(() =>
            Variant.RepetitionEvaluator.WouldCreateThreefoldRepetition(
                game.State,
                illegalMove));

        snapshot.AssertMatches(game);
    }

    [Fact]
    public void UndoAndReexecuteNaturallyChangeRepetitionFacts()
    {
        var game = Variant.CreateGame();

        PlayInitialPositionCycle(game);
        PlayInitialPositionCycle(game);
        Assert.True(
            Evaluate(game)
                .IsThreefoldRepetition);

        var lastMove = game.UndoLastMove()
            .Execution.Move;

        Assert.False(
            Evaluate(game)
                .IsThreefoldRepetition);

        game.Execute(lastMove);

        Assert.True(
            Evaluate(game)
                .IsThreefoldRepetition);
    }

    private static StandardRepetitionFacts Evaluate(
        Game game)
    {
        return Variant.RepetitionEvaluator.Evaluate(game.State);
    }

    private static StandardRepetitionEvaluator CreateEvaluator(
        GameVariantDefinition definition)
    {
        return new StandardRepetitionEvaluator(
            Variant.PositionFactsEvaluator,
            definition.CreateGame);
    }

    private static void PlayInitialPositionCycle(
        Game game)
    {
        TestSupport.Play(game, "g1", "f3");
        TestSupport.Play(game, "g8", "f6");
        TestSupport.Play(game, "f3", "g1");
        TestSupport.Play(game, "f6", "g8");
    }
}
