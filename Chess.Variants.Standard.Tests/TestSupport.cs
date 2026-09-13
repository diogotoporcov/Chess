// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Games.Attacks;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Board.Topology;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Games.Rules;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Movement.Orientation;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Tests;

internal static class TestSupport
{
    public static Square Square(
        string coordinate)
    {
        if (coordinate.Length != 2 ||
            coordinate[0] is < 'a' or > 'h' ||
            coordinate[1] is < '1' or > '8')
        {
            throw new ArgumentException(
                "Use algebraic coordinates from a1 through h8.",
                nameof(coordinate));
        }

        var column = coordinate[0] - 'a';
        var row = '8' - coordinate[1];

        return BoardLayout.GetSquare(row, column);
    }

    public static Placement At(
        string coordinate,
        Side side,
        PieceDefinition definition)
    {
        return new Placement(Square(coordinate), side, definition);
    }

    public static Game CreateGame(
        params Placement[] placements)
    {
        return CreateGame(
            new TurnOrder(SideDefinitions.White, SideDefinitions.Black),
            placements);
    }

    public static Game CreateGame(
        TurnOrder turnOrder,
        params Placement[] placements)
    {
        return CreateDefinition(turnOrder, placements)
            .CreateGame();
    }

    public static GameVariantDefinition CreateDefinition(
        TurnOrder turnOrder,
        params Placement[] placements)
    {
        var executionResolver = CreateExecutionResolver();
        var simulator = new GameMoveSimulator(executionResolver);
        var checkDetector = new CheckDetector(new PatternAttackGenerator());
        var pseudoLegalGenerator = new CastlingMoveGenerator(
            new EnPassantMoveGenerator(
                new PromotionMoveGenerator(new PseudoLegalGameMoveGenerator())),
            simulator,
            checkDetector);
        var legalGenerator = new LegalMoveGenerator(
            pseudoLegalGenerator,
            simulator,
            checkDetector);

        return new GameVariantDefinition(
            new GameVariantId("test:standard-position"),
            "Standard test position",
            BoardTopologyFactory.Create(),
            turnOrder,
            Orientations.Resolver,
            BoardRegions.Resolver,
            legalGenerator,
            executionResolver,
            new StatusEvaluator(legalGenerator, checkDetector),
            placements
                .Select(placement => new InitialPiecePlacement(
                    placement.Square,
                    placement.Side,
                    placement.Definition))
                .ToArray());
    }

    public static IMoveExecutionResolver CreateExecutionResolver()
    {
        return new CompositeMoveExecutionResolver(
            new BasicMoveExecutionResolver(),
            new PromotionMoveExecutionResolver(),
            new EnPassantMoveExecutionResolver(),
            new CastlingMoveExecutionResolver());
    }

    public static Move FindMove(
        Game game,
        string from,
        string to,
        MoveOptionId? option = null)
    {
        var expected = new Move(Square(from), Square(to), option);
        return Assert.Single(
            game.GenerateMoves(expected.From),
            move => move == expected);
    }

    public static GameMoveRecord Play(
        Game game,
        string from,
        string to,
        MoveOptionId? option = null)
    {
        return game.Execute(FindMove(game, from, to, option));
    }

    public static IReadOnlyList<Move> AllMoves(
        Game game)
    {
        return game
            .BoardState
            .GetPiecePositions(game.State.CurrentSide)
            .SelectMany(position => game.GenerateMoves(position.Square))
            .ToArray();
    }

    public static Piece PieceAt(
        Game game,
        string coordinate)
    {
        Assert.True(
            game.BoardState.TryGetPiece(Square(coordinate), out var piece));
        return piece;
    }
}

internal readonly record struct Placement(
    Square Square,
    Side Side,
    PieceDefinition Definition);

internal sealed record StandardGameSnapshot(
    Side CurrentSide,
    IReadOnlyList<GameMoveRecord> History,
    IReadOnlyDictionary<Square, Piece> Pieces)
{
    public static StandardGameSnapshot Capture(
        Game game)
    {
        var pieces = game
            .BoardState
            .Topology
            .Squares
            .Where(square => game.BoardState.TryGetPiece(square, out _))
            .ToDictionary(
                square => square,
                square =>
                {
                    game.BoardState.TryGetPiece(square, out var piece);
                    return piece!;
                });

        return new StandardGameSnapshot(
            game.State.CurrentSide,
            game.State.History.ToArray(),
            pieces);
    }

    public void AssertMatches(
        Game game)
    {
        Assert.Equal(CurrentSide, game.State.CurrentSide);
        Assert.Equal(History.Count, game.State.History.Count);

        for (var index = 0; index < History.Count; index++)
        {
            Assert.Same(History[index], game.State.History[index]);
        }

        foreach (var square in game.BoardState.Topology.Squares)
        {
            var expectedOccupied = Pieces.TryGetValue(square, out var expected);
            var actualOccupied = game.BoardState.TryGetPiece(
                square,
                out var actual);

            Assert.Equal(expectedOccupied, actualOccupied);

            if (expectedOccupied)
            {
                Assert.Same(expected, actual);
                Assert.True(
                    game.BoardState.TryGetSquare(expected!, out var at));
                Assert.Equal(square, at);
            }
        }
    }
}
