using System.Collections.ObjectModel;

namespace Chess.Desktop.Presentation;

public sealed class BoardPresentation
{
    private readonly ReadOnlyCollection<BoardSquarePresentation>
        _squares;

    public string Summary { get; }

    public double SquareSize { get; }

    public double Width { get; }

    public double Height { get; }

    public IReadOnlyList<BoardSquarePresentation>
        Squares => _squares;

    public BoardPresentation(
        string summary,
        double squareSize,
        IEnumerable<BoardSquarePresentation> squares)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException(
                "Board presentation summary cannot be empty.",
                nameof(summary));
        }

        if (squareSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(squareSize),
                "Square size must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(squares);

        var squareArray = squares.ToArray();

        if (squareArray.Length == 0)
        {
            throw new ArgumentException(
                "Board presentation must contain at least one square.",
                nameof(squares));
        }

        if (squareArray
            .GroupBy(square => square.Square)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Board presentation cannot contain the same square more than once.",
                nameof(squares));
        }

        Summary = summary.Trim();
        SquareSize = squareSize;

        Width =
            squareArray.Max(square => square.X) +
            squareSize;

        Height =
            squareArray.Max(square => square.Y) +
            squareSize;

        _squares =
            Array.AsReadOnly(squareArray);
    }
}
