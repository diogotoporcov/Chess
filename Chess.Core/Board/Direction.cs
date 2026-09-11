namespace Chess.Core.Board;

public readonly record struct Direction
{
    public string Name { get; }

    public Direction(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Direction name cannot be empty.",
                nameof(name));
        }

        Name = name.Trim();
    }

    public override string ToString()
    {
        return Name;
    }
}