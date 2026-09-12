using Chess.Core.Games;
using Chess.Core.Games.Attacks;
using Chess.Core.Games.Variants;
using Chess.Core.Movement;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Board.Topology;
using Chess.Variants.Standard.Games;
using Chess.Variants.Standard.Games.Rules;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Movement.Orientation;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

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
        var basicExecutionResolver = new BasicMoveExecutionResolver();
        
        var executionResolver = new MoveExecutionResolver(basicExecutionResolver);

        var attackGenerator = new PatternAttackGenerator();

        var checkDetector = new CheckDetector(attackGenerator);

        var pseudoLegalMoveGenerator =
            new PromotionMoveGenerator(
                new PseudoLegalGameMoveGenerator());

        var legalMoveGenerator =
            new LegalMoveGenerator(
                pseudoLegalMoveGenerator,
                new GameMoveSimulator(
                    executionResolver),
                checkDetector);

        var statusEvaluator =
            new StatusEvaluator(
                legalMoveGenerator,
                checkDetector);

        return new GameVariantDefinition(
            new GameVariantId("chess:standard"),
            "Standard Chess",
            BoardTopologyFactory.Create(),
            TurnOrderDefinition.Instance,
            Orientations.Resolver,
            BoardRegions.Resolver,
            legalMoveGenerator,
            executionResolver,
            statusEvaluator,
            CreateInitialPlacements());
    }

    private static InitialPiecePlacement[]
        CreateInitialPlacements()
    {
        var placements = new List<InitialPiecePlacement>();

        AddPawns(
            placements,
            SideDefinitions.Black,
            BlackPawnRow);

        AddPawns(
            placements,
            SideDefinitions.White,
            WhitePawnRow);

        AddBackRank(
            placements,
            SideDefinitions.Black,
            BlackBackRankRow);

        AddBackRank(
            placements,
            SideDefinitions.White,
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