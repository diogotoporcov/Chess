// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Board.Topology;
using Chess.Core.Games;
using Chess.Core.Games.Status;
using Chess.Core.Movement;
using Chess.Core.Movement.Conditions;
using Chess.Core.Movement.Orientation;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Core.Tests;

internal static class TestSupport
{
    public static readonly Side White = new("test:white");
    public static readonly Side Black = new("test:black");

    public static readonly PieceDefinition Token = new(
        new PieceDefinitionId("test:token"),
        "Token");

    public static BoardTopology CreateGrid(
        int rows = 5,
        int columns = 5)
    {
        var builder = new BoardTopologyBuilder();

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                builder.AddSquare(At(row, column, columns));
            }
        }

        var directions = new[]
        {
            (-1, 0, CompassDirections.North),
            (-1, 1, CompassDirections.NorthEast),
            (0, 1, CompassDirections.East),
            (1, 1, CompassDirections.SouthEast),
            (1, 0, CompassDirections.South),
            (1, -1, CompassDirections.SouthWest),
            (0, -1, CompassDirections.West),
            (-1, -1, CompassDirections.NorthWest)
        };

        for (var row = 0; row < rows; row++)
        {
            for (var column = 0; column < columns; column++)
            {
                foreach (var (rowOffset, columnOffset, direction) in directions)
                {
                    var targetRow = row + rowOffset;
                    var targetColumn = column + columnOffset;

                    if (targetRow >= 0 &&
                        targetRow < rows &&
                        targetColumn >= 0 &&
                        targetColumn < columns)
                    {
                        builder.Connect(
                            At(row, column, columns),
                            direction,
                            At(targetRow, targetColumn, columns));
                    }
                }
            }
        }

        return builder.Build();
    }

    public static Square At(
        int row,
        int column,
        int columns = 5)
    {
        return new Square(row * columns + column);
    }

    public static GameState CreateState(
        BoardTopology topology,
        TurnOrder? turnOrder = null,
        params (Square Square, Piece Piece)[] placements)
    {
        var boardBuilder = new BoardStateBuilder(topology);

        foreach (var (square, piece) in placements)
        {
            boardBuilder.PlacePiece(square, piece);
        }

        var context = new MovementContext(
            boardBuilder.Build(),
            new ThrowingOrientationResolver(),
            new EmptyRegionResolver());

        return new GameState(context, turnOrder ?? new TurnOrder(White, Black));
    }

    public static Piece Piece(
        Side? side = null,
        PieceDefinition? definition = null)
    {
        return new Piece(side ?? White, definition ?? Token);
    }
}

internal sealed record GameStateSnapshot(
    Side CurrentSide,
    IReadOnlyList<GameMoveRecord> History,
    IReadOnlyDictionary<Square, Piece> Pieces)
{
    public static GameStateSnapshot Capture(
        GameState state)
    {
        var pieces = state
            .BoardState
            .Topology
            .Squares
            .Where(square => state.BoardState.TryGetPiece(square, out _))
            .ToDictionary(
                square => square,
                square =>
                {
                    state.BoardState.TryGetPiece(square, out var piece);
                    return piece!;
                });

        return new GameStateSnapshot(
            state.CurrentSide,
            state.History.ToArray(),
            pieces);
    }

    public void AssertMatches(
        GameState state)
    {
        Assert.Equal(CurrentSide, state.CurrentSide);
        Assert.Equal(History.Count, state.History.Count);

        for (var index = 0; index < History.Count; index++)
        {
            Assert.Same(History[index], state.History[index]);
        }

        foreach (var square in state.BoardState.Topology.Squares)
        {
            var expectedOccupied = Pieces.TryGetValue(square, out var expected);
            var actualOccupied = state.BoardState.TryGetPiece(
                square,
                out var actual);

            Assert.Equal(expectedOccupied, actualOccupied);

            if (expectedOccupied)
            {
                Assert.Same(expected, actual);
                Assert.True(
                    state.BoardState.TryGetSquare(expected!, out var at));
                Assert.Equal(square, at);
            }
        }
    }
}

internal sealed class FixedMoveGenerator(params Move[] moves)
    : IGameMoveGenerator
{
    public IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        return moves.Where(move => move.From == from);
    }
}

internal sealed class DelegateResolver(
    Func<GameState, Move, MoveExecution> resolve,
    Func<Move, bool>? canResolve = null) : IMoveExecutionResolver
{
    public bool CanResolve(
        Move move)
    {
        return canResolve?.Invoke(move) ?? true;
    }

    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        return resolve(gameState, move);
    }
}

internal sealed class EmptyRegionResolver : IBoardRegionResolver
{
    public bool Contains(
        Side side,
        BoardRegionId regionId,
        Square square)
    {
        return false;
    }
}

internal sealed class ThrowingOrientationResolver : IRelativeDirectionResolver
{
    public Direction Resolve(
        Side side,
        RelativeDirection relativeDirection)
    {
        throw new InvalidOperationException(
            "No relative directions configured.");
    }
}

internal sealed class FixedCondition(bool result) : IMovementCondition
{
    public bool IsSatisfied(
        MovementContext context,
        Square from,
        Side movingSide)
    {
        return result;
    }
}

internal sealed class FixedStatusEvaluator(GameStatus status)
    : IGameStatusEvaluator
{
    public GameStatus Evaluate(
        GameState gameState)
    {
        return status;
    }
}
