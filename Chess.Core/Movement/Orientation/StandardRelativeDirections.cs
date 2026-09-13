namespace Chess.Core.Movement.Orientation;

public static class StandardRelativeDirections
{
    public static readonly RelativeDirection Forward = new("Forward");
    public static readonly RelativeDirection ForwardRight = new("ForwardRight");
    public static readonly RelativeDirection Right = new("Right");

    public static readonly RelativeDirection BackwardRight =
        new("BackwardRight");

    public static readonly RelativeDirection Backward = new("Backward");
    public static readonly RelativeDirection BackwardLeft = new("BackwardLeft");
    public static readonly RelativeDirection Left = new("Left");
    public static readonly RelativeDirection ForwardLeft = new("ForwardLeft");
}
