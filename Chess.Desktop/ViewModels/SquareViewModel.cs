using Chess.Core.Board;

namespace Chess.Desktop.ViewModels;

public sealed class SquareViewModel : ViewModelBase
{
    private string? _pieceImageSource;

    private bool _isSelected;

    private bool _isLegalDestination;

    private bool _isCaptureDestination;

    public Square Square { get; }

    public double X { get; }

    public double Y { get; }

    public double Size { get; }

    public double PieceSize { get; }

    public double SelectionMarkerSize { get; }

    public double LegalMoveMarkerSize { get; }

    public double CaptureMarkerSize { get; }

    public double CaptureMarkerStrokeThickness { get; }

    public bool IsLightSquare { get; }

    public string? PieceImageSource
    {
        get => _pieceImageSource;

        set => SetProperty(ref _pieceImageSource, value);
    }

    public bool IsSelected
    {
        get => _isSelected;

        set => SetProperty(ref _isSelected, value);
    }

    public bool IsLegalDestination
    {
        get => _isLegalDestination;

        set => SetProperty(ref _isLegalDestination, value);
    }

    public bool IsCaptureDestination
    {
        get => _isCaptureDestination;

        set => SetProperty(ref _isCaptureDestination, value);
    }

    public SquareViewModel(
        Square square,
        double x,
        double y,
        double size,
        bool isLightSquare)
    {
        if (size <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(size),
                "Square size must be greater than zero.");
        }

        Square = square;
        X = x;
        Y = y;
        Size = size;

        PieceSize = size * 7.0 / 9.0;

        SelectionMarkerSize = size * 0.84;

        LegalMoveMarkerSize = size * 0.18;

        CaptureMarkerSize = size * 0.88;

        CaptureMarkerStrokeThickness = size * 0.045;

        IsLightSquare = isLightSquare;
    }
}
