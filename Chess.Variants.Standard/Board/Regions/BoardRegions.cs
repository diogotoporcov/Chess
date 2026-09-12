using Chess.Core.Board.Regions;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Board.Regions;

public static class BoardRegions
{
    private const int BlackPawnStartingRow = 1;
    private const int WhitePawnStartingRow = 6;

    private const int BlackPromotionRow = 7;
    private const int WhitePromotionRow = 0;

    public static readonly BoardRegionId PawnStarting = new("chess:pawn-start");

    public static readonly BoardRegionId Promotion = new("chess:promotion");

    public static IBoardRegionResolver Resolver { get; } =
        new SideBoardRegionMap(
            (
                SideDefinitions.White,
                PawnStarting,
                CreateRow(
                    WhitePawnStartingRow)
            ),
            (
                SideDefinitions.Black,
                PawnStarting,
                CreateRow(
                    BlackPawnStartingRow)
            ),
            (
                SideDefinitions.White,
                Promotion,
                CreateRow(
                    WhitePromotionRow)
            ),
            (
                SideDefinitions.Black,
                Promotion,
                CreateRow(
                    BlackPromotionRow)
            ));

    private static BoardRegion CreateRow(
        int row)
    {
        return new BoardRegion(
            BoardGeometry.GetRow(
                row));
    }
}