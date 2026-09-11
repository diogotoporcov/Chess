using Chess.Core.Movement.Orientation;
using Chess.Core.Board;

namespace Chess.Variants.Standard.Movement.Orientation;

public static class Orientations
{
    public static IRelativeDirectionResolver Resolver { get; } =
        new SideOrientationMap(
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.Forward,
                CompassDirections.North
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.ForwardRight,
                CompassDirections.NorthEast
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.Right,
                CompassDirections.East
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.BackwardRight,
                CompassDirections.SouthEast
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.Backward,
                CompassDirections.South
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.BackwardLeft,
                CompassDirections.SouthWest
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.Left,
                CompassDirections.West
            ),
            (
                Sides.SideDefinitions.White,
                StandardRelativeDirections.ForwardLeft,
                CompassDirections.NorthWest
            ),

            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.Forward,
                CompassDirections.South
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.ForwardRight,
                CompassDirections.SouthWest
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.Right,
                CompassDirections.West
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.BackwardRight,
                CompassDirections.NorthWest
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.Backward,
                CompassDirections.North
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.BackwardLeft,
                CompassDirections.NorthEast
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.Left,
                CompassDirections.East
            ),
            (
                Sides.SideDefinitions.Black,
                StandardRelativeDirections.ForwardLeft,
                CompassDirections.SouthEast
            ));
}