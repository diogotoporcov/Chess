using Chess.Core.Board.Regions;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Board.Regions;

public static class StandardChessBoardRegions
{
    private const int BlackPawnStartingRow = 1;
    private const int WhitePawnStartingRow = 6;

    public static readonly BoardRegionId PawnStarting = new("chess:pawn-start");

    public static IBoardRegionResolver Resolver { get; } =
        new SideBoardRegionMap(
            (
                StandardSides.White,
                PawnStarting,
                CreateRow(WhitePawnStartingRow)
            ),
            (
                StandardSides.Black,
                PawnStarting,
                CreateRow(BlackPawnStartingRow)
            ));

    private static BoardRegion CreateRow(
        int row)
    {
        return new BoardRegion(
            StandardChessBoardGeometry.GetRow(
                row));
    }
}