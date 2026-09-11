using Chess.Core.Board.Regions;

namespace Chess.Variants.Standard.Board.Regions;

public static class BoardRegions
{
    private const int BlackPawnStartingRow = 1;
    private const int WhitePawnStartingRow = 6;

    public static readonly BoardRegionId PawnStarting = new("chess:pawn-start");

    public static IBoardRegionResolver Resolver { get; } =
        new SideBoardRegionMap(
            (
                Sides.SideDefinitions.White,
                PawnStarting,
                CreateRow(WhitePawnStartingRow)
            ),
            (
                Sides.SideDefinitions.Black,
                PawnStarting,
                CreateRow(BlackPawnStartingRow)
            ));

    private static BoardRegion CreateRow(
        int row)
    {
        return new BoardRegion(
            BoardGeometry.GetRow(
                row));
    }
}