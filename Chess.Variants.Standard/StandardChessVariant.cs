using Chess.Core.Games;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Board.Topology;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Movement.Orientation;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard;

public static class StandardChessVariant
{
    private const int BlackBackRankRow = 0;
    private const int BlackPawnRow = 1;

    private const int WhitePawnRow = 6;
    private const int WhiteBackRankRow = 7;

    public static GameVariantDefinition Definition { get; } = CreateDefinition();

    public static Game CreateGame()
    {
        return Definition.CreateGame();
    }

    private static GameVariantDefinition CreateDefinition()
    {
        return new GameVariantDefinition(
            new GameVariantId("chess:standard"),
            "Standard Chess",
            StandardChessBoardTopology.Create(),
            StandardChessTurnOrder.Instance,
            StandardChessOrientations.Resolver,
            StandardChessBoardRegions.Resolver,
            new PseudoLegalGameMoveGenerator(),
            new BasicMoveExecutionResolver(),
            CreateInitialPlacements());
    }

    private static InitialPiecePlacement[]
        CreateInitialPlacements()
    {
        var placements =
            new List<InitialPiecePlacement>();

        AddPawns(
            placements,
            StandardSides.Black,
            BlackPawnRow);

        AddPawns(
            placements,
            StandardSides.White,
            WhitePawnRow);

        AddBackRank(
            placements,
            StandardSides.Black,
            BlackBackRankRow);

        AddBackRank(
            placements,
            StandardSides.White,
            WhiteBackRankRow);

        return
        [
            .. placements
        ];
    }

    private static void AddPawns(
        ICollection<InitialPiecePlacement> placements,
        Side side,
        int row)
    {
        for (var column = 0; column < StandardChessBoardGeometry.SideDimension; column++)
        {
            placements.Add(
                new InitialPiecePlacement(
                    StandardChessBoardGeometry.SquareAt(
                        row,
                        column),
                    side,
                    StandardPieceDefinitions.Pawn));
        }
    }

    private static void AddBackRank(
        ICollection<InitialPiecePlacement> placements,
        Side side,
        int row)
    {
        var definitions = new[]
        {
            StandardPieceDefinitions.Rook,
            StandardPieceDefinitions.Knight,
            StandardPieceDefinitions.Bishop,
            StandardPieceDefinitions.Queen,
            StandardPieceDefinitions.King,
            StandardPieceDefinitions.Bishop,
            StandardPieceDefinitions.Knight,
            StandardPieceDefinitions.Rook
        };

        for (var column = 0; column < definitions.Length; column++)
        {
            placements.Add(
                new InitialPiecePlacement(
                    StandardChessBoardGeometry.SquareAt(
                        row,
                        column),
                    side,
                    definitions[column]));
        }
    }
}