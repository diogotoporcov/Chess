using Chess.Core.Board;
using Chess.Core.Board.Topology;

namespace Chess.Variants.Standard.Board.Topology;

public static class BoardTopologyFactory
{
    public static BoardTopology Create()
    {
        var builder =
            new BoardTopologyBuilder();

        for (var row = 0; row < BoardGeometry.SideDimension; row++)
        {
            for (var column = 0; column < BoardGeometry.SideDimension; column++)
            {
                builder.AddSquare(
                    BoardGeometry.SquareAt(
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
        for (var row = 0; row < BoardGeometry.SideDimension; row++)
        {
            for (var column = 0; column < BoardGeometry.SideDimension; column++)
            {
                var from =
                    BoardGeometry.SquareAt(
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
        if (!BoardGeometry.Contains(
                targetRow,
                targetColumn))
        {
            return;
        }

        builder.Connect(
            from,
            direction,
            BoardGeometry.SquareAt(
                targetRow,
                targetColumn));
    }
}