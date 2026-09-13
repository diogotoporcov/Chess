// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Movement;
using Chess.Core.Movement.Conditions;
using Chess.Core.Movement.Orientation;
using Chess.Core.Movement.Patterns;
using Chess.Core.Pieces;
using Chess.Variants.Standard.Board.Regions;

namespace Chess.Variants.Standard.Pieces;

public static class PieceDefinitions
{
    public static PieceDefinition Pawn { get; } = new(
        new PieceDefinitionId("chess:pawn"),
        "Pawn",
        new SlidingMovementPattern(
            StandardRelativeDirections.Forward,
            maxDistance: 1,
            targetMode: MovementTargetMode.MoveOnly),
        new SlidingMovementPattern(
            StandardRelativeDirections.ForwardLeft,
            maxDistance: 1,
            targetMode: MovementTargetMode.CaptureOnly),
        new SlidingMovementPattern(
            StandardRelativeDirections.ForwardRight,
            maxDistance: 1,
            targetMode: MovementTargetMode.CaptureOnly),
        new ConditionalMovementPattern(
            new OriginInRegionCondition(BoardRegions.PawnStarting),
            new PathMovementPattern(
                MovementTargetMode.MoveOnly,
                StandardRelativeDirections.Forward,
                StandardRelativeDirections.Forward)));

    public static PieceDefinition Rook { get; } = new(
        new PieceDefinitionId("chess:rook"),
        "Rook",
        new SlidingMovementPattern(CompassDirections.North),
        new SlidingMovementPattern(CompassDirections.East),
        new SlidingMovementPattern(CompassDirections.South),
        new SlidingMovementPattern(CompassDirections.West));

    public static PieceDefinition Bishop { get; } = new(
        new PieceDefinitionId("chess:bishop"),
        "Bishop",
        new SlidingMovementPattern(CompassDirections.NorthEast),
        new SlidingMovementPattern(CompassDirections.SouthEast),
        new SlidingMovementPattern(CompassDirections.SouthWest),
        new SlidingMovementPattern(CompassDirections.NorthWest));

    public static PieceDefinition Queen { get; } = new(
        new PieceDefinitionId("chess:queen"),
        "Queen",
        new SlidingMovementPattern(CompassDirections.North),
        new SlidingMovementPattern(CompassDirections.NorthEast),
        new SlidingMovementPattern(CompassDirections.East),
        new SlidingMovementPattern(CompassDirections.SouthEast),
        new SlidingMovementPattern(CompassDirections.South),
        new SlidingMovementPattern(CompassDirections.SouthWest),
        new SlidingMovementPattern(CompassDirections.West),
        new SlidingMovementPattern(CompassDirections.NorthWest));

    public static PieceDefinition King { get; } = new(
        new PieceDefinitionId("chess:king"),
        "King",
        new SlidingMovementPattern(CompassDirections.North, maxDistance: 1),
        new SlidingMovementPattern(CompassDirections.NorthEast, maxDistance: 1),
        new SlidingMovementPattern(CompassDirections.East, maxDistance: 1),
        new SlidingMovementPattern(CompassDirections.SouthEast, maxDistance: 1),
        new SlidingMovementPattern(CompassDirections.South, maxDistance: 1),
        new SlidingMovementPattern(CompassDirections.SouthWest, maxDistance: 1),
        new SlidingMovementPattern(CompassDirections.West, maxDistance: 1),
        new SlidingMovementPattern(
            CompassDirections.NorthWest,
            maxDistance: 1));

    public static PieceDefinition Knight { get; } = new(
        new PieceDefinitionId("chess:knight"),
        "Knight",
        CreateKnightLeap(CompassDirections.North, CompassDirections.East),
        CreateKnightLeap(CompassDirections.North, CompassDirections.West),
        CreateKnightLeap(CompassDirections.South, CompassDirections.East),
        CreateKnightLeap(CompassDirections.South, CompassDirections.West),
        CreateKnightLeap(CompassDirections.East, CompassDirections.North),
        CreateKnightLeap(CompassDirections.East, CompassDirections.South),
        CreateKnightLeap(CompassDirections.West, CompassDirections.North),
        CreateKnightLeap(CompassDirections.West, CompassDirections.South));

    private static LeapingMovementPattern CreateKnightLeap(
        Direction primaryDirection,
        Direction secondaryDirection)
    {
        return new LeapingMovementPattern(
            new Displacement(
                new DisplacementComponent(primaryDirection, 2),
                new DisplacementComponent(secondaryDirection, 1)));
    }
}
