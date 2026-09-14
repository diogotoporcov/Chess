// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games.Rules;

internal static class EnPassantRules
{
    public static IEnumerable<Move> GenerateMoves(
        StandardEnPassantTargetEvaluator targetEvaluator,
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(targetEvaluator);
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(from, out var pawn))
        {
            yield break;
        }

        if (pawn.Side != gameState.CurrentSide ||
            pawn.Definition.Id != PieceDefinitions.Pawn.Id)
        {
            yield break;
        }

        foreach (var destinationDirection in
                 GetDestinationDirections(pawn.Side))
        {
            if (!gameState.BoardState.Topology.TryGetNext(
                    from,
                    destinationDirection,
                    out var destination))
            {
                continue;
            }

            var move = new Move(from, destination, MoveOptions.EnPassant);

            if (TryGetCapturedPawn(
                    targetEvaluator,
                    gameState,
                    move,
                    out _,
                    out _))
            {
                yield return move;
            }
        }
    }

    public static bool TryGetCapturedPawn(
        StandardEnPassantTargetEvaluator targetEvaluator,
        GameState gameState,
        Move move,
        out Square capturedPawnSquare,
        [NotNullWhen(true)] out Piece? capturedPawn)
    {
        ArgumentNullException.ThrowIfNull(targetEvaluator);
        ArgumentNullException.ThrowIfNull(gameState);

        capturedPawnSquare = default;
        capturedPawn = null;

        if (move.OptionId != MoveOptions.EnPassant)
        {
            return false;
        }

        var boardState = gameState.BoardState;

        if (!boardState.TryGetPiece(move.From, out var movingPawn))
        {
            return false;
        }

        if (movingPawn.Side != gameState.CurrentSide ||
            !ReferenceEquals(movingPawn.Definition, PieceDefinitions.Pawn))
        {
            return false;
        }

        if (boardState.IsOccupied(move.To))
        {
            return false;
        }

        if (targetEvaluator.Evaluate(gameState) != move.To)
        {
            return false;
        }

        if (!TryGetAdjacentCaptureSquare(
                gameState,
                movingPawn.Side,
                move.From,
                move.To,
                out capturedPawnSquare))
        {
            return false;
        }

        if (!boardState.TryGetPiece(capturedPawnSquare, out capturedPawn))
        {
            return false;
        }

        if (capturedPawn.Side == movingPawn.Side ||
            !ReferenceEquals(capturedPawn.Definition, PieceDefinitions.Pawn))
        {
            capturedPawn = null;
            return false;
        }

        return true;
    }

    private static bool TryGetAdjacentCaptureSquare(
        GameState gameState,
        Side movingSide,
        Square from,
        Square destination,
        out Square adjacentSquare)
    {
        var topology = gameState.BoardState.Topology;

        foreach (var (horizontalDirection, destinationDirection) in
                 GetCaptureDirections(movingSide))
        {
            if (!topology.TryGetNext(
                    from,
                    destinationDirection,
                    out var expectedDestination) ||
                expectedDestination != destination)
            {
                continue;
            }

            if (!topology.TryGetNext(
                    from,
                    horizontalDirection,
                    out adjacentSquare))
            {
                continue;
            }

            return true;
        }

        adjacentSquare = default;

        return false;
    }

    private static IEnumerable<Direction> GetDestinationDirections(
        Side side)
    {
        if (side == SideDefinitions.White)
        {
            yield return CompassDirections.NorthWest;
            yield return CompassDirections.NorthEast;
            yield break;
        }

        if (side == SideDefinitions.Black)
        {
            yield return CompassDirections.SouthWest;
            yield return CompassDirections.SouthEast;
            yield break;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }

    private static IEnumerable<( Direction Horizontal, Direction Destination)>
        GetCaptureDirections(
            Side side)
    {
        if (side == SideDefinitions.White)
        {
            yield return (CompassDirections.West, CompassDirections.NorthWest);

            yield return (CompassDirections.East, CompassDirections.NorthEast);

            yield break;
        }

        if (side == SideDefinitions.Black)
        {
            yield return (CompassDirections.West, CompassDirections.SouthWest);

            yield return (CompassDirections.East, CompassDirections.SouthEast);

            yield break;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }
}
