using Chess.Core.Movement;
using Chess.Core.Sides;

namespace Chess.Core.Games;

public sealed record GameMoveRecord
{
    public int PlyNumber { get; }

    public Side Side { get; }

    public MoveExecution Execution { get; }

    internal GameMoveRecord(
        int plyNumber,
        Side side,
        MoveExecution execution)
    {
        if (plyNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(plyNumber),
                "Ply number must be greater than zero.");
        }

        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(execution);

        PlyNumber = plyNumber;
        Side = side;
        Execution = execution;
    }
}