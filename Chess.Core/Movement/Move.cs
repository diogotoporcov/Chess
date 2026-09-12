using Chess.Core.Board;

namespace Chess.Core.Movement;

public readonly record struct Move
{
    public Square From { get; }

    public Square To { get; }

    public MoveOptionId? OptionId { get; }

    public Move(
        Square from,
        Square to,
        MoveOptionId? optionId = null)
    {
        if (from == to)
        {
            throw new ArgumentException(
                "Move origin and destination must be different.");
        }

        From = from;
        To = to;
        OptionId = optionId;
    }
}