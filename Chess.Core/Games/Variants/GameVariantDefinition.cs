using Chess.Core.Board;
using Chess.Core.Board.Regions;
using Chess.Core.Board.Topology;
using Chess.Core.Games.Status;
using Chess.Core.Movement;
using Chess.Core.Movement.Orientation;
using Chess.Core.Pieces;

namespace Chess.Core.Games.Variants;

public sealed class GameVariantDefinition
{
    private readonly IReadOnlyList<InitialPiecePlacement> _initialPlacements;

    private readonly IRelativeDirectionResolver _relativeDirectionResolver;

    private readonly IBoardRegionResolver _boardRegionResolver;

    private readonly IGameMoveGenerator _moveGenerator;

    private readonly IMoveExecutionResolver _moveExecutionResolver;

    private readonly IGameStatusEvaluator _statusEvaluator;

    public GameVariantId Id { get; }

    public string Name { get; }

    public BoardTopology Topology { get; }

    public TurnOrder TurnOrder { get; }

    public IReadOnlyList<InitialPiecePlacement> InitialPlacements => _initialPlacements;

    public GameVariantDefinition(
        GameVariantId id,
        string name,
        BoardTopology topology,
        TurnOrder turnOrder,
        IRelativeDirectionResolver relativeDirectionResolver,
        IBoardRegionResolver boardRegionResolver,
        IGameMoveGenerator moveGenerator,
        IMoveExecutionResolver moveExecutionResolver,
        IGameStatusEvaluator statusEvaluator,
        params InitialPiecePlacement[] initialPlacements)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Game variant name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(turnOrder);
        ArgumentNullException.ThrowIfNull(relativeDirectionResolver);
        ArgumentNullException.ThrowIfNull(boardRegionResolver);
        ArgumentNullException.ThrowIfNull(moveGenerator);
        ArgumentNullException.ThrowIfNull(moveExecutionResolver);
        ArgumentNullException.ThrowIfNull(statusEvaluator);
        ArgumentNullException.ThrowIfNull(initialPlacements);

        if (initialPlacements
            .GroupBy(placement => placement.Square)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Initial piece placements cannot contain multiple pieces on the same square.",
                nameof(initialPlacements));
        }

        foreach (var placement in initialPlacements)
        {
            if (!topology.Contains(placement.Square))
            {
                throw new ArgumentException(
                    "An initial piece placement references a square outside the topology.",
                    nameof(initialPlacements));
            }

            if (!turnOrder.Contains(placement.Side))
            {
                throw new ArgumentException(
                    "An initial piece placement references a side outside the turn order.",
                    nameof(initialPlacements));
            }
        }

        Id = id;
        Name = name.Trim();

        Topology = topology;
        TurnOrder = turnOrder;

        _relativeDirectionResolver = relativeDirectionResolver;

        _boardRegionResolver = boardRegionResolver;

        _moveGenerator = moveGenerator;

        _moveExecutionResolver = moveExecutionResolver;

        _statusEvaluator = statusEvaluator;

        _initialPlacements = Array.AsReadOnly(
                [
                    .. initialPlacements
                ]);
    }

    public Game CreateGame()
    {
        var boardState = new BoardState(Topology);

        foreach (var placement in _initialPlacements)
        {
            boardState.PlacePiece(
                placement.Square,
                new Piece(
                    placement.Side,
                    placement.Definition));
        }

        var movementContext =
            new MovementContext(
                boardState,
                _relativeDirectionResolver,
                _boardRegionResolver);

        var gameState =
            new GameState(
                movementContext,
                TurnOrder);

        var moveExecutor =
            new GameMoveExecutor(
                _moveGenerator,
                _moveExecutionResolver);

        return new Game(
            this,
            gameState,
            _moveGenerator,
            moveExecutor,
            _statusEvaluator);
    }

    public override string ToString()
    {
        return Name;
    }
}