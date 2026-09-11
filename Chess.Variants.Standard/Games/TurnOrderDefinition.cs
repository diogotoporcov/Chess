namespace Chess.Variants.Standard.Games;

public static class TurnOrderDefinition
{
    public static Core.Games.TurnOrder Instance { get; } =
        new(
            Sides.SideDefinitions.White,
            Sides.SideDefinitions.Black);
}