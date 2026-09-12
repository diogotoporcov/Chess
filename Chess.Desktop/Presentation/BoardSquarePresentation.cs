using Chess.Core.Board;

namespace Chess.Desktop.Presentation;

public sealed record BoardSquarePresentation
{
    public Square Square { get; }

    public double X { get; }

    public double Y { get; }

    public bool IsLightSquare { get; }

    public BoardSquarePresentation(
        Square square,
        double x,
        double y,
        bool isLightSquare)
    {
        if (x < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(x),
                "Square X coordinate cannot be negative.");
        }

        if (y < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(y),
                "Square Y coordinate cannot be negative.");
        }

        Square = square;
        X = x;
        Y = y;
        IsLightSquare = isLightSquare;
    }
}
