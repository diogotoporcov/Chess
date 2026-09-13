using System.Collections.ObjectModel;
using Chess.Desktop.GameModes;

namespace Chess.Desktop.ViewModels;

public sealed class ModeSelectionViewModel : ViewModelBase
{
    private readonly ReadOnlyCollection<ModeOptionViewModel> _modes;

    public IReadOnlyList<ModeOptionViewModel> Modes => _modes;

    public ModeSelectionViewModel(
        GameModeCatalog catalog,
        Action<GameModeDefinition> startMode)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(startMode);

        _modes = Array.AsReadOnly(
            catalog
                .Modes
                .Select(mode => new ModeOptionViewModel(mode, startMode))
                .ToArray());
    }
}
