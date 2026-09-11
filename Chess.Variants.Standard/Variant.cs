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

namespace Chess.Variants.Standard;

public static class Variant
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
            BoardTopologyFactory.Create(),
            TurnOrderDefinition.Instance,
            Orientations.Resolver,
            BoardRegions.Resolver,
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
            Sides.SideDefinitions.Black,
            BlackPawnRow);

        AddPawns(
            placements,
            Sides.SideDefinitions.White,
            WhitePawnRow);

        AddBackRank(
            placements,
            Sides.SideDefinitions.Black,
            BlackBackRankRow);

        AddBackRank(
            placements,
            Sides.SideDefinitions.White,
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
        for (var column = 0; column < BoardGeometry.SideDimension; column++)
        {
            placements.Add(
                new InitialPiecePlacement(
                    BoardGeometry.SquareAt(
                        row,
                        column),
                    side,
                    PieceDefinitions.Pawn));
        }
    }

    private static void AddBackRank(
        ICollection<InitialPiecePlacement> placements,
        Side side,
        int row)
    {
        var definitions = new[]
        {
            PieceDefinitions.Rook,
            PieceDefinitions.Knight,
            PieceDefinitions.Bishop,
            PieceDefinitions.Queen,
            PieceDefinitions.King,
            PieceDefinitions.Bishop,
            PieceDefinitions.Knight,
            PieceDefinitions.Rook
        };

        for (var column = 0; column < definitions.Length; column++)
        {
            placements.Add(
                new InitialPiecePlacement(
                    BoardGeometry.SquareAt(
                        row,
                        column),
                    side,
                    definitions[column]));
        }
    }
}