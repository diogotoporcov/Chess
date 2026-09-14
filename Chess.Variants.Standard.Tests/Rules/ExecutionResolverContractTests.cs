// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests.Rules;

public sealed class ExecutionResolverContractTests
{
    [Fact]
    public void SpecializedResolversRejectMovesForOtherOptions()
    {
        var ordinary = new Move(
            TestSupport.Square("a2"),
            TestSupport.Square("a3"));
        var state = Variant.CreateGame()
            .State;

        Assert.Throws<InvalidOperationException>(() => TestSupport
            .CreateCastlingMoveExecutionResolver()
            .Resolve(state, ordinary));
        Assert.Throws<InvalidOperationException>(() => TestSupport
            .CreateEnPassantMoveExecutionResolver()
            .Resolve(state, ordinary));
        Assert.Throws<InvalidOperationException>(() =>
            new PromotionMoveExecutionResolver().Resolve(state, ordinary));
    }

    [Fact]
    public void CastlingResolverRejectsStructurallyInvalidCastle()
    {
        var state = Variant.CreateGame()
            .State;
        var move = new Move(
            TestSupport.Square("e1"),
            TestSupport.Square("g1"),
            MoveOptions.CastleKingSide);

        Assert.Throws<InvalidOperationException>(() => TestSupport
            .CreateCastlingMoveExecutionResolver()
            .Resolve(state, move));
    }

    [Fact]
    public void EnPassantResolverRejectsExpiredOpportunity()
    {
        var game = TestSupport.CreateGame(
            TestSupport.At("e1", SideDefinitions.White, PieceDefinitions.King),
            TestSupport.At("e8", SideDefinitions.Black, PieceDefinitions.King),
            TestSupport.At("e5", SideDefinitions.White, PieceDefinitions.Pawn),
            TestSupport.At("d5", SideDefinitions.Black, PieceDefinitions.Pawn));
        var move = new Move(
            TestSupport.Square("e5"),
            TestSupport.Square("d6"),
            MoveOptions.EnPassant);

        Assert.Throws<InvalidOperationException>(() => TestSupport
            .CreateEnPassantMoveExecutionResolver()
            .Resolve(game.State, move));
    }

    [Theory]
    [InlineData("empty origin")]
    [InlineData("non-pawn")]
    [InlineData("wrong rank")]
    [InlineData("friendly destination")]
    public void PromotionResolverRejectsInvalidPromotionState(
        string invalidState)
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
                PieceDefinitions.King)
        };
        var from = invalidState == "wrong rank" ? "a6" : "a7";
        var to = invalidState == "wrong rank" ? "a7" : "a8";

        if (invalidState != "empty origin")
        {
            placements.Add(
                TestSupport.At(
                    from,
                    SideDefinitions.White,
                    invalidState == "non-pawn"
                        ? PieceDefinitions.Rook
                        : PieceDefinitions.Pawn));
        }

        if (invalidState == "friendly destination")
        {
            placements.Add(
                TestSupport.At(
                    to,
                    SideDefinitions.White,
                    PieceDefinitions.Knight));
        }

        var game = TestSupport.CreateGame(placements.ToArray());
        var move = new Move(
            TestSupport.Square(from),
            TestSupport.Square(to),
            PromotionOptions.Queen);

        Assert.Throws<InvalidOperationException>(() =>
            new PromotionMoveExecutionResolver().Resolve(game.State, move));
    }
}
