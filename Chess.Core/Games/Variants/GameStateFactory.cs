// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Board.Topology;
using Chess.Core.Movement;
using Chess.Core.Movement.Orientation;
using Chess.Core.Pieces;

namespace Chess.Core.Games.Variants;

public sealed class GameStateFactory
{
    private readonly IRelativeDirectionResolver _relativeDirectionResolver;

    private readonly IBoardRegionResolver _boardRegionResolver;

    public BoardTopology Topology { get; }

    public TurnOrder TurnOrder { get; }

    public IReadOnlyList<InitialPiecePlacement> InitialPlacements { get; }

    public GameStateFactory(
        BoardTopology topology,
        TurnOrder turnOrder,
        IRelativeDirectionResolver relativeDirectionResolver,
        IBoardRegionResolver boardRegionResolver,
        params InitialPiecePlacement[] initialPlacements)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(turnOrder);
        ArgumentNullException.ThrowIfNull(relativeDirectionResolver);
        ArgumentNullException.ThrowIfNull(boardRegionResolver);
        ArgumentNullException.ThrowIfNull(initialPlacements);

        if (Array.IndexOf(initialPlacements, null!) >= 0)
        {
            throw new ArgumentException(
                "Initial piece placements cannot contain null elements.",
                nameof(initialPlacements));
        }

        if (initialPlacements
            .GroupBy(placement => placement.Square)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Initial piece placements cannot contain multiple pieces " +
                "on the same square.",
                nameof(initialPlacements));
        }

        foreach (var placement in initialPlacements)
        {
            if (!topology.Contains(placement.Square))
            {
                throw new ArgumentException(
                    "An initial piece placement references a square outside " +
                    "the topology.",
                    nameof(initialPlacements));
            }

            if (!turnOrder.Contains(placement.Side))
            {
                throw new ArgumentException(
                    "An initial piece placement references a side outside " +
                    "the turn order.",
                    nameof(initialPlacements));
            }
        }

        Topology = topology;
        TurnOrder = turnOrder;

        _relativeDirectionResolver = relativeDirectionResolver;
        _boardRegionResolver = boardRegionResolver;
        InitialPlacements = Array.AsReadOnly([.. initialPlacements]);
    }

    public GameState Create()
    {
        var boardBuilder = new BoardStateBuilder(Topology);

        foreach (var placement in InitialPlacements)
        {
            boardBuilder.PlacePiece(
                placement.Square,
                new Piece(placement.Side, placement.Definition));
        }

        var movementContext = new MovementContext(
            boardBuilder.Build(),
            _relativeDirectionResolver,
            _boardRegionResolver);

        return new GameState(movementContext, TurnOrder);
    }
}
