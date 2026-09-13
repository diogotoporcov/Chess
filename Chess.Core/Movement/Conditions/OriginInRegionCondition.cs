using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Conditions;

public sealed class OriginInRegionCondition : IMovementCondition
{
    private readonly BoardRegionId _regionId;

    public OriginInRegionCondition(
        BoardRegionId regionId)
    {
        ArgumentNullException.ThrowIfNull(regionId);

        _regionId = regionId;
    }

    public bool IsSatisfied(
        MovementContext context,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(movingSide);

        return context.IsInRegion(movingSide, _regionId, from);
    }
}
