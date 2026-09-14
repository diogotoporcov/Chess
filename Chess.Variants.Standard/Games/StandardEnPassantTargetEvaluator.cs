// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games;

public sealed class StandardEnPassantTargetEvaluator
{
    private readonly Square? _initialTarget;

    public StandardEnPassantTargetEvaluator(
        Square? initialTarget)
    {
        _initialTarget = initialTarget;
    }

    public Square? Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (gameState.History.Count == 0)
        {
            return _initialTarget;
        }

        var lastMove = gameState.LastMove!;
        var move = lastMove.Execution.Move;

        if (move.OptionId is not null)
        {
            return null;
        }

        var originChange =
            lastMove.Execution.Transition.Changes.SingleOrDefault(change =>
                change.Square == move.From);
        var destinationChange =
            lastMove.Execution.Transition.Changes.SingleOrDefault(change =>
                change.Square == move.To);
        var pawn = originChange?.Before;

        if (pawn is null ||
            !ReferenceEquals(pawn.Definition, PieceDefinitions.Pawn) ||
            lastMove.Side != pawn.Side ||
            originChange!.After is not null ||
            destinationChange is null ||
            destinationChange.Before is not null ||
            !ReferenceEquals(destinationChange.After, pawn) ||
            !gameState.MovementContext.IsInRegion(
                pawn.Side,
                BoardRegions.PawnStarting,
                move.From))
        {
            return null;
        }

        var topology = gameState.BoardState.Topology;
        var forward = GetForwardDirection(pawn.Side);

        if (!topology.TryGetNext(move.From, forward, out var target) ||
            !topology.TryGetNext(target, forward, out var destination) ||
            destination != move.To)
        {
            return null;
        }

        return target;
    }

    private static Direction GetForwardDirection(
        Side side)
    {
        if (side == SideDefinitions.White)
        {
            return CompassDirections.North;
        }

        if (side == SideDefinitions.Black)
        {
            return CompassDirections.South;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }
}
