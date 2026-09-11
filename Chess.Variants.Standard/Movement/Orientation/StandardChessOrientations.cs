using Chess.Variants.Standard.Sides;
using Chess.Core.Movement.Orientation;
using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Variants.Standard.Movement.Orientation;

public static class StandardChessOrientations
{
    public static IRelativeDirectionResolver Resolver { get; } =
        new SideOrientationMap(
            (
                StandardSides.White,
                StandardRelativeDirections.Forward,
                CompassDirections.North
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.ForwardRight,
                CompassDirections.NorthEast
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.Right,
                CompassDirections.East
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.BackwardRight,
                CompassDirections.SouthEast
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.Backward,
                CompassDirections.South
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.BackwardLeft,
                CompassDirections.SouthWest
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.Left,
                CompassDirections.West
            ),
            (
                StandardSides.White,
                StandardRelativeDirections.ForwardLeft,
                CompassDirections.NorthWest
            ),

            (
                StandardSides.Black,
                StandardRelativeDirections.Forward,
                CompassDirections.South
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.ForwardRight,
                CompassDirections.SouthWest
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.Right,
                CompassDirections.West
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.BackwardRight,
                CompassDirections.NorthWest
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.Backward,
                CompassDirections.North
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.BackwardLeft,
                CompassDirections.NorthEast
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.Left,
                CompassDirections.East
            ),
            (
                StandardSides.Black,
                StandardRelativeDirections.ForwardLeft,
                CompassDirections.SouthEast
            ));
}