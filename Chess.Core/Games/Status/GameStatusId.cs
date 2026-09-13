namespace Chess.Core.Games.Status;

public sealed record GameStatusId
{
    public string Value { get; }

    public GameStatusId(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "Game status id cannot be empty.",
                nameof(value));
        }

        Value = value.Trim();
    }

    public override string ToString()
    {
        return Value;
    }
}
