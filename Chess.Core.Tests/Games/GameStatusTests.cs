// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games.Status;
using Chess.Core.Sides;

namespace Chess.Core.Tests.Games;

public sealed class GameStatusTests
{
    [Fact]
    public void GameTerminationIdRejectsEmptyValuesAndTrimsAcceptedValue()
    {
        Assert.Throws<ArgumentException>(() => new GameTerminationId(" "));

        var id = new GameTerminationId(" test:finished ");

        Assert.Equal("test:finished", id.Value);
        Assert.Equal(id.Value, id.ToString());
    }

    [Fact]
    public void GameOutcomeRejectsNullTerminationAndWinners()
    {
        var termination = new GameTerminationId("test:finished");

        Assert.Throws<ArgumentNullException>(() => new GameOutcome(null!));
        Assert.Throws<ArgumentNullException>(() =>
            new GameOutcome(termination, null!));
    }

    [Fact]
    public void GameOutcomeRejectsDuplicateEqualWinners()
    {
        var termination = new GameTerminationId("test:won");

        Assert.Throws<ArgumentException>(() => new GameOutcome(
            termination,
            TestSupport.White,
            new Side(TestSupport.White.Id)));
    }

    [Fact]
    public void GameOutcomeRejectsNullWinnerElement()
    {
        var termination = new GameTerminationId("test:won");
        Side[] winners = [null!];

        Assert.Throws<ArgumentNullException>(() =>
            new GameOutcome(termination, winners));
    }

    [Fact]
    public void GameOutcomeAllowsZeroOneOrMultipleDistinctWinners()
    {
        var termination = new GameTerminationId("test:finished");
        var third = new Side("test:third");

        Assert.Empty(new GameOutcome(termination).Winners);
        Assert.Equal(
            [TestSupport.White],
            new GameOutcome(termination, TestSupport.White).Winners);
        Assert.Equal(
            [TestSupport.White, TestSupport.Black, third],
            new GameOutcome(
                termination,
                TestSupport.White,
                TestSupport.Black,
                third).Winners);
    }

    [Fact]
    public void GameOutcomeWinnersCannotBeMutatedThroughSourceOrExposedView()
    {
        var termination = new GameTerminationId("test:won");
        Side[] winners = [TestSupport.White];
        var outcome = new GameOutcome(termination, winners);

        winners[0] = TestSupport.Black;

        Assert.Equal([TestSupport.White], outcome.Winners);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<Side>)outcome.Winners)[0] = TestSupport.Black);
    }

    [Fact]
    public void GameStatusDerivesTerminalityFromOutcomePresence()
    {
        var statusId = new GameStatusId("test:active");
        var ongoing = new GameStatus(statusId);
        var outcome = new GameOutcome(new GameTerminationId("test:finished"));
        var terminal = new GameStatus(statusId, outcome);

        Assert.Null(ongoing.Outcome);
        Assert.False(ongoing.IsTerminal);
        Assert.Same(outcome, terminal.Outcome);
        Assert.True(terminal.IsTerminal);
    }
}
