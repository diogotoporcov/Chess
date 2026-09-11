namespace Chess.Core.Games.Variants;

public sealed record GameVariantId
{
    public string Value { get; }

    public GameVariantId(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Game variant id cannot be empty.",
                nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}