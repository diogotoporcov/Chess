namespace Chess.Core.Board.Regions;

public sealed record BoardRegionId
{
    public string Value { get; }

    public BoardRegionId(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Board region id cannot be empty.",
                nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
