using Chess.Core.Board.Topology;
using Chess.Core.Board;
namespace Chess.Variants.Standard.Board.Topology;

public static class StandardChessBoardTopology
{
    private const int BoardSideDimension = 8;

    public static BoardTopology Create()
    {
        var builder = new BoardTopologyBuilder();

        for (var id = 0;
             id < BoardSideDimension * BoardSideDimension;
             id++)
        {
            builder.AddSquare(new Square(id));
        }

        ConnectSquares(builder);

        return builder.Build();
    }

    private static void ConnectSquares(
        BoardTopologyBuilder builder)
    {
        for (var id = 0;
             id < BoardSideDimension * BoardSideDimension;
             id++)
        {
            var row = id / BoardSideDimension;
            var column = id % BoardSideDimension;

            var from = new Square(id);

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

    private static void TryConnect(
        BoardTopologyBuilder builder,
        Square from,
        int targetRow,
        int targetColumn,
        Direction direction)
    {
        if (targetRow < 0 ||
            targetRow >= BoardSideDimension ||
            targetColumn < 0 ||
            targetColumn >= BoardSideDimension)
        {
            return;
        }

        var targetId =
            targetRow * BoardSideDimension + targetColumn;

        builder.Connect(
            from,
            direction,
            new Square(targetId));
    }
}