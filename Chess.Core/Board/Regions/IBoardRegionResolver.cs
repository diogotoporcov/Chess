using Chess.Core.Sides;

namespace Chess.Core.Board.Regions;

public interface IBoardRegionResolver
{
    bool Contains(
        Side side,
        BoardRegionId regionId,
        Square square);
}