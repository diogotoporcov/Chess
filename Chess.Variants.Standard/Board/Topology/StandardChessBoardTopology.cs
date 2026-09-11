using Chess.Core.Board;
using Chess.Core.Board.Topology;

namespace Chess.Variants.Standard.Board.Topology;

public static class StandardChessBoardTopology
{
    public static BoardTopology Create()
    {
        var builder =
            new BoardTopologyBuilder();

        for (var row = 0; row < StandardChessBoardGeometry.SideDimension; row++)
        {
            for (var column = 0; column < StandardChessBoardGeometry.SideDimension; column++)
            {
                builder.AddSquare(
                    StandardChessBoardGeometry.SquareAt(
                        row,
                        column));
            }
        }

        ConnectSquares(builder);

        return builder.Build();
    }

    private static void ConnectSquares(
        BoardTopologyBuilder builder)
    {
        for (var row = 0; row < StandardChessBoardGeometry.SideDimension; row++)
        {
            for (var column = 0; column < StandardChessBoardGeometry.SideDimension; column++)
            {
                var from =
                    StandardChessBoardGeometry.SquareAt(
                        row,
                        column);

                TryConnect(
                    builder,
                    from,
                    row - 1,
                    column,
                    CompassDirections.North);

                TryConnect(
                    builder,
                    from,
                    row - 1,
                    column + 1,
                    CompassDirections.NorthEast);

                TryConnect(
                    builder,
                    from,
                    row,
                    column + 1,
                    CompassDirections.East);

                TryConnect(
                    builder,
                    from,
                    row + 1,
                    column + 1,
                    CompassDirections.SouthEast);

                TryConnect(
                    builder,
                    from,
                    row + 1,
                    column,
                    CompassDirections.South);

                TryConnect(
                    builder,
                    from,
                    row + 1,
                    column - 1,
                    CompassDirections.SouthWest);

                TryConnect(
                    builder,
                    from,
                    row,
                    column - 1,
                    CompassDirections.West);

                TryConnect(
                    builder,
                    from,
                    row - 1,
                    column - 1,
                    CompassDirections.NorthWest);
            }
        }
    }

    private static void TryConnect(
        BoardTopologyBuilder builder,
        Square from,
        int targetRow,
        int targetColumn,
        Direction direction)
    {
        if (!StandardChessBoardGeometry.Contains(
                targetRow,
                targetColumn))
        {
            return;
        }

        builder.Connect(
            from,
            direction,
            StandardChessBoardGeometry.SquareAt(
                targetRow,
                targetColumn));
    }
}