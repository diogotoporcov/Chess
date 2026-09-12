using Chess.Core.Board;

namespace Chess.Variants.Standard.Board;

public static class BoardLayout
{
    public const int Rows = BoardGeometry.SideDimension;
    public const int Columns = BoardGeometry.SideDimension;

    public static Square GetSquare(
        int row,
        int column)
    {
        return BoardGeometry.SquareAt(
            row,
            column);
    }

    public static int GetRow(
        Square square)
    {
        EnsureValidSquare(square);

        return square.Id / BoardGeometry.SideDimension;
    }

    public static int GetColumn(
        Square square)
    {
        EnsureValidSquare(square);

        return square.Id % BoardGeometry.SideDimension;
    }

    private static void EnsureValidSquare(
        Square square)
    {
        const int squareCount = BoardGeometry.SideDimension * BoardGeometry.SideDimension;

        if (square.Id is < 0 or >= squareCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(square),
                "Square is not part of the standard chess board.");
        }
    }
}