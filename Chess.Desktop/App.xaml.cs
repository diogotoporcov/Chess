using System.Windows;
using Chess.Desktop.Composition;

namespace Chess.Desktop;

public partial class App
{
    protected override void OnStartup(
        StartupEventArgs e)
    {
        base.OnStartup(e);

        var window = new MainWindow
        {
            DataContext = AppComposition.CreateShell()
        };

        MainWindow = window;
        window.Show();
    }
}