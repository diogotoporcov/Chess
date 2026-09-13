// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Board.Topology;
using Chess.Core.Movement.Orientation;
using Chess.Core.Sides;

namespace Chess.Core.Tests.Board;

public sealed class TopologyAndRegionsTests
{
    [Fact]
    public void Topology_ContainsAddedSquaresAndFollowsConnections()
    {
        var builder = new BoardTopologyBuilder();
        var left = new Square(10);
        var right = new Square(20);

        Assert.True(builder.AddSquare(left));
        Assert.False(builder.AddSquare(left));
        builder.AddSquare(right);
        builder.Connect(left, CompassDirections.East, right);

        var topology = builder.Build();

        Assert.Equal(2, topology.Squares.Count);
        Assert.True(topology.Contains(left));
        Assert.True(
            topology.TryGetNext(left, CompassDirections.East, out var next));
        Assert.Equal(right, next);
        Assert.False(topology.TryGetNext(right, CompassDirections.East, out _));
    }

    [Fact]
    public void Connect_RejectsUnknownEndpointsAndDuplicateDirection()
    {
        var builder = new BoardTopologyBuilder();
        var known = new Square(1);
        var destination = new Square(2);
        var unknown = new Square(99);
        builder.AddSquare(known);
        builder.AddSquare(destination);

        Assert.Throws<ArgumentException>(() =>
            builder.Connect(unknown, CompassDirections.East, known));
        Assert.Throws<ArgumentException>(() =>
            builder.Connect(known, CompassDirections.East, unknown));

        builder.Connect(known, CompassDirections.East, destination);

        Assert.Throws<InvalidOperationException>(() =>
            builder.Connect(known, CompassDirections.East, known));
    }

    [Fact]
    public void TryGetNext_RejectsASourceOutsideTheTopology()
    {
        var topology = TestSupport.CreateGrid(1, 1);

        Assert.Throws<ArgumentException>(() => topology.TryGetNext(
            new Square(5),
            CompassDirections.North,
            out _));
    }

    [Fact]
    public void BoardRegion_UsesSetMembershipAndRequiresASquare()
    {
        var square = new Square(1);
        var region = new BoardRegion([square, square]);

        Assert.True(region.Contains(square));
        Assert.False(region.Contains(new Square(2)));
        Assert.Throws<ArgumentException>(() => new BoardRegion([]));
    }

    [Fact]
    public void SideBoardRegionMap_ResolvesBySideAndRejectsDuplicateMappings()
    {
        var side = new Side("north");
        var regionId = new BoardRegionId("home");
        var square = new Square(1);
        var region = new BoardRegion([square]);
        var map = new SideBoardRegionMap((side, regionId, region));

        Assert.True(map.Contains(side, regionId, square));
        Assert.Throws<InvalidOperationException>(() =>
            map.Contains(new Side("south"), regionId, square));
        Assert.Throws<ArgumentException>(() => new SideBoardRegionMap(
            (side, regionId, region),
            (side, regionId, region)));
    }

    [Fact]
    public void
        SideOrientationMap_ResolvesAndRejectsMissingOrDuplicateMappings()
    {
        var side = new Side("north");
        var relative = new RelativeDirection("ahead");
        var map = new SideOrientationMap(
            (side, relative, CompassDirections.North));

        Assert.Same(CompassDirections.North, map.Resolve(side, relative));
        Assert.Throws<InvalidOperationException>(() =>
            map.Resolve(side, new RelativeDirection("behind")));
        Assert.Throws<ArgumentException>(() => new SideOrientationMap(
            (side, relative, CompassDirections.North),
            (side, relative, CompassDirections.South)));
    }
}
