// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board.Regions;
using Chess.Core.Board.Topology;
using Chess.Core.Games.Status;
using Chess.Core.Movement;
using Chess.Core.Movement.Orientation;

namespace Chess.Core.Games.Variants;

public sealed class GameVariantDefinition
{
    private readonly GameStateFactory _gameStateFactory;

    private readonly IGameMoveGenerator _moveGenerator;

    private readonly GameMoveResolver _moveResolver;

    private readonly IGameStatusEvaluator _statusEvaluator;

    public GameVariantId Id { get; }

    public string Name { get; }

    public BoardTopology Topology => _gameStateFactory.Topology;

    public TurnOrder TurnOrder => _gameStateFactory.TurnOrder;

    public IReadOnlyList<InitialPiecePlacement> InitialPlacements =>
        _gameStateFactory.InitialPlacements;

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
        params InitialPiecePlacement[] initialPlacements) : this(
        id,
        name,
        new GameStateFactory(
            topology,
            turnOrder,
            relativeDirectionResolver,
            boardRegionResolver,
            initialPlacements),
        moveGenerator,
        moveExecutionResolver,
        statusEvaluator)
    {
    }

    public GameVariantDefinition(
        GameVariantId id,
        string name,
        GameStateFactory gameStateFactory,
        IGameMoveGenerator moveGenerator,
        IMoveExecutionResolver moveExecutionResolver,
        IGameStatusEvaluator statusEvaluator)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Game variant name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(gameStateFactory);
        ArgumentNullException.ThrowIfNull(moveGenerator);
        ArgumentNullException.ThrowIfNull(moveExecutionResolver);
        ArgumentNullException.ThrowIfNull(statusEvaluator);

        Id = id;
        Name = name.Trim();

        _gameStateFactory = gameStateFactory;
        _moveGenerator = moveGenerator;

        _moveResolver = new GameMoveResolver(
            moveGenerator,
            moveExecutionResolver);

        _statusEvaluator = statusEvaluator;
    }

    public Game CreateGame()
    {
        var gameState = _gameStateFactory.Create();

        var moveExecutor = new GameMoveExecutor(_moveResolver);

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
