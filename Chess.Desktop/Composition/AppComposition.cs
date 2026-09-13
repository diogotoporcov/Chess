using Chess.Desktop.GameModes;
using Chess.Desktop.GameModes.Standard;
using Chess.Desktop.ViewModels;

namespace Chess.Desktop.Composition;

public static class AppComposition
{
    public static ShellViewModel CreateShell()
    {
        IGameModeProvider[] providers = [new StandardGameModeProvider()];

        var catalog = new GameModeCatalog(providers);

        return new ShellViewModel(catalog);
    }
}
