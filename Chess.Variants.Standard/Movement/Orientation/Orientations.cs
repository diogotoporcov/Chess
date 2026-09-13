// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Movement.Orientation;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Movement.Orientation;

public static class Orientations
{
    public static IRelativeDirectionResolver Resolver { get; } =
        new SideOrientationMap(
            (SideDefinitions.White, StandardRelativeDirections.Forward,
                CompassDirections.North),
            (SideDefinitions.White, StandardRelativeDirections.ForwardRight,
                CompassDirections.NorthEast),
            (SideDefinitions.White, StandardRelativeDirections.Right,
                CompassDirections.East),
            (SideDefinitions.White, StandardRelativeDirections.BackwardRight,
                CompassDirections.SouthEast),
            (SideDefinitions.White, StandardRelativeDirections.Backward,
                CompassDirections.South),
            (SideDefinitions.White, StandardRelativeDirections.BackwardLeft,
                CompassDirections.SouthWest),
            (SideDefinitions.White, StandardRelativeDirections.Left,
                CompassDirections.West),
            (SideDefinitions.White, StandardRelativeDirections.ForwardLeft,
                CompassDirections.NorthWest),
            (SideDefinitions.Black, StandardRelativeDirections.Forward,
                CompassDirections.South),
            (SideDefinitions.Black, StandardRelativeDirections.ForwardRight,
                CompassDirections.SouthWest),
            (SideDefinitions.Black, StandardRelativeDirections.Right,
                CompassDirections.West),
            (SideDefinitions.Black, StandardRelativeDirections.BackwardRight,
                CompassDirections.NorthWest),
            (SideDefinitions.Black, StandardRelativeDirections.Backward,
                CompassDirections.North),
            (SideDefinitions.Black, StandardRelativeDirections.BackwardLeft,
                CompassDirections.NorthEast),
            (SideDefinitions.Black, StandardRelativeDirections.Left,
                CompassDirections.East),
            (SideDefinitions.Black, StandardRelativeDirections.ForwardLeft,
                CompassDirections.SouthEast));
}
