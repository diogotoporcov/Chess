using Chess.Desktop.GameModes;

namespace Chess.Desktop.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly GameModeCatalog _catalog;

    private ViewModelBase _currentScreen = null!;

    public ViewModelBase CurrentScreen
    {
        get => _currentScreen;
        private set => SetProperty(ref _currentScreen, value);
    }

    public ShellViewModel(
        GameModeCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        _catalog = catalog;

        ShowModeSelection();
    }

    private void ShowModeSelection()
    {
        CurrentScreen = new ModeSelectionViewModel(_catalog, StartGame);
    }

    private void StartGame(
        GameModeDefinition mode)
    {
        CurrentScreen = new GameViewModel(mode, ShowModeSelection);
    }
}
