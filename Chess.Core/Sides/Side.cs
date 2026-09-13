namespace Chess.Core.Sides;

public sealed record Side
{
    public string Id { get; }

    public Side(
        string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Side id cannot be empty.", nameof(id));
        }

        Id = id.Trim();
    }

    public override string ToString()
    {
        return Id;
    }
}
