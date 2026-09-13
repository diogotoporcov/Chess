// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Movement.Orientation;
using Chess.Core.Sides;

namespace Chess.Core.Movement;

public sealed class MovementContext
{
    private readonly IRelativeDirectionResolver _relativeDirectionResolver;

    private readonly IBoardRegionResolver _boardRegionResolver;

    public BoardState BoardState { get; }

    public MovementContext(
        BoardState boardState,
        IRelativeDirectionResolver relativeDirectionResolver,
        IBoardRegionResolver boardRegionResolver)
    {
        ArgumentNullException.ThrowIfNull(boardState);
        ArgumentNullException.ThrowIfNull(relativeDirectionResolver);
        ArgumentNullException.ThrowIfNull(boardRegionResolver);

        BoardState = boardState;

        _relativeDirectionResolver = relativeDirectionResolver;

        _boardRegionResolver = boardRegionResolver;
    }

    public bool IsInRegion(
        Side side,
        BoardRegionId regionId,
        Square square)
    {
        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(regionId);

        if (!BoardState.Topology.Contains(square))
        {
            throw new ArgumentException(
                "Square is not part of the board.",
                nameof(square));
        }

        return _boardRegionResolver.Contains(side, regionId, square);
    }

    internal Direction ResolveRelativeDirection(
        Side side,
        RelativeDirection relativeDirection)
    {
        return _relativeDirectionResolver.Resolve(side, relativeDirection);
    }
}
