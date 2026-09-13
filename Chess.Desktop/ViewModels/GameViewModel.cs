using System.Collections.ObjectModel;
using System.Windows.Input;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Desktop.GameModes;
using Chess.Desktop.Infrastructure.Commands;

namespace Chess.Desktop.ViewModels;

public sealed class GameViewModel : ViewModelBase
{
    private readonly GameModeDefinition _mode;

    private readonly Game _game;

    private SquareViewModel? _selectedSquare;

    private IReadOnlyList<Move> _selectedMoves = [];

    private readonly ReadOnlyCollection<SquareViewModel> _squares;

    public IReadOnlyList<SquareViewModel> Squares => _squares;

    public double BoardWidth => _mode.Presentation.Board.Width;

    public double BoardHeight => _mode.Presentation.Board.Height;

    public string ModeName => _game.Variant.Name;

    public string CurrentTurn =>
        _mode.Presentation.GetSideName(_game.State.CurrentSide);

    public string Status => _mode.Presentation.GetStatusName(_game.Status.Id);

    public ICommand SelectSquareCommand { get; }

    public ICommand BackCommand { get; }

    public GameViewModel(
        GameModeDefinition mode,
        Action goBack)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(goBack);

        _mode = mode;
        _game = mode.CreateGame();

        _squares = Array.AsReadOnly(CreateSquares());

        SelectSquareCommand = new RelayCommand<SquareViewModel>(SelectSquare);

        BackCommand = new RelayCommand(goBack);

        RefreshBoard();
    }

    private SquareViewModel[] CreateSquares()
    {
        var board = _mode.Presentation.Board;

        return
        [
            .. board.Squares.Select(square => new SquareViewModel(
                square.Square,
                square.X,
                square.Y,
                board.SquareSize,
                square.IsLightSquare))
        ];
    }

    private void SelectSquare(
        SquareViewModel square)
    {
        ArgumentNullException.ThrowIfNull(square);

        if (ReferenceEquals(square, _selectedSquare))
        {
            ClearSelection();
            return;
        }

        var destinationMove = _selectedSquare is null
            ? null
            : _selectedMoves
                .Where(candidate => candidate.To == square.Square)
                .Select(candidate => (Move?)candidate)
                .FirstOrDefault();

        if (destinationMove is { } selectedMove)
        {
            ExecuteMove(selectedMove);
            return;
        }

        if (!_game.BoardState.TryGetPiece(square.Square, out var piece) ||
            piece.Side != _game.State.CurrentSide)
        {
            ClearSelection();
            return;
        }

        _selectedSquare = square;

        _selectedMoves = [.. _game.GenerateMoves(square.Square)];

        RefreshSelection();
    }

    private void ExecuteMove(
        Move move)
    {
        _game.Execute(move);

        ClearSelection();
        RefreshBoard();

        OnPropertyChanged(nameof(CurrentTurn));

        OnPropertyChanged(nameof(Status));
    }

    private void RefreshBoard()
    {
        foreach (var square in _squares)
        {
            if (!_game.BoardState.TryGetPiece(square.Square, out var piece))
            {
                square.PieceImageSource = null;
                continue;
            }

            square.PieceImageSource =
                _mode.Presentation.GetPieceImageSource(piece);
        }
    }

    private void ClearSelection()
    {
        _selectedSquare = null;
        _selectedMoves = [];

        RefreshSelection();
    }

    private void RefreshSelection()
    {
        var legalDestinations = _selectedMoves
            .Select(candidate => candidate.To)
            .ToHashSet();

        foreach (var square in _squares)
        {
            square.IsSelected = ReferenceEquals(square, _selectedSquare);

            var isLegalDestination = legalDestinations.Contains(square.Square);

            var isOccupied = isLegalDestination &&
                             _game.BoardState.IsOccupied(square.Square);

            square.IsLegalDestination = isLegalDestination && !isOccupied;

            square.IsCaptureDestination = isLegalDestination && isOccupied;
        }
    }
}
