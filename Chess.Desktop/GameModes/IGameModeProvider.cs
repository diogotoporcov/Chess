namespace Chess.Desktop.GameModes;

public interface IGameModeProvider
{
    IEnumerable<GameModeDefinition> GetModes();
}
