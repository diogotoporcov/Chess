using Chess.Core.Board;

namespace Chess.Variants.Standard.Board;

internal static class StandardChessBoardGeometry
{
    public const int SideDimension = 8;

    public static bool Contains(
        int row,
        int column)
    {
        return row is >= 0 and < SideDimension &&
               column is >= 0 and < SideDimension;
    }

    public static Square SquareAt(
        int row,
        int column)
    {
        if (!Contains(
                row,
                column))
        {
            throw new ArgumentOutOfRangeException(
                nameof(row),
                "Board coordinates must be inside the standard 8x8 board.");
        }

        return new Square(
            row * SideDimension +
            column);
    }

    public static IEnumerable<Square> GetRow(
        int row)
    {
        if (row is < 0 or >= SideDimension)
        {
            throw new ArgumentOutOfRangeException(
                nameof(row),
                "Board row must be inside the standard 8x8 board.");
        }

        for (var column = 0; column < SideDimension; column++)
        {
            yield return SquareAt(
                row,
                column);
        }
    }
}