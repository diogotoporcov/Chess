// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardPositionFactsEvaluator
{
    private readonly IGameMoveGenerator _legalMoveGenerator;

    public StandardPositionFactsEvaluator(
        IGameMoveGenerator legalMoveGenerator)
    {
        ArgumentNullException.ThrowIfNull(legalMoveGenerator);

        _legalMoveGenerator = legalMoveGenerator;
    }

    public StandardPositionFacts Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var castlingRights = CastlingRightsEvaluator.Evaluate(gameState);
        var effectiveEnPassantTarget =
            EvaluateEffectiveEnPassantTarget(gameState);
        var key = new StandardPositionKey(
            EvaluatePiecePlacements(gameState),
            gameState.CurrentSide.Id,
            castlingRights,
            effectiveEnPassantTarget);

        return new StandardPositionFacts(key, EvaluateHalfmoveClock(gameState));
    }

    public StandardPositionKey CreatePositionKey(
        GameState gameState)
    {
        return Evaluate(gameState)
            .PositionKey;
    }

    public int EvaluateHalfmoveClock(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var halfmoveClock = 0;

        for (var index = gameState.History.Count - 1; index >= 0; index--)
        {
            var execution = gameState.History[index].Execution;
            var isPawnMove = IsPawnMove(execution);
            var isCapture = IsCapture(execution);

            if (isPawnMove || isCapture)
            {
                break;
            }

            halfmoveClock++;
        }

        return halfmoveClock;
    }

    private static IReadOnlyList<StandardPiecePlacement>
        EvaluatePiecePlacements(
            GameState gameState)
    {
        var placements = new List<StandardPiecePlacement>();

        foreach (var square in gameState.BoardState.Topology.Squares)
        {
            if (!gameState.BoardState.TryGetPiece(square, out var piece))
            {
                continue;
            }

            placements.Add(
                new StandardPiecePlacement(
                    square,
                    piece.Side.Id,
                    piece.Definition.Id.Value));
        }

        return placements;
    }

    private Square? EvaluateEffectiveEnPassantTarget(
        GameState gameState)
    {
        var targets = gameState
            .BoardState
            .GetPiecePositions(gameState.CurrentSide)
            .SelectMany(position => _legalMoveGenerator.GenerateMoves(
                gameState,
                position.Square))
            .Where(move => move.OptionId == MoveOptions.EnPassant)
            .Select(move => move.To)
            .Distinct()
            .ToArray();

        return targets.Length switch
        {
            0 => null,
            1 => targets[0],
            _ => throw new InvalidOperationException(
                "A standard position cannot have multiple en passant " +
                "target squares.")
        };
    }

    private static bool IsPawnMove(
        MoveExecution execution)
    {
        var originChange =
            execution.Transition.Changes.SingleOrDefault(change =>
                change.Square == execution.Move.From);

        if (originChange?.Before is null)
        {
            throw new InvalidOperationException(
                "Move execution does not identify the moving piece " +
                "at its origin.");
        }

        return originChange.Before.Definition.Id == PieceDefinitions.Pawn.Id;
    }

    private static bool IsCapture(
        MoveExecution execution)
    {
        foreach (var change in execution.Transition.Changes)
        {
            if (change.Square == execution.Move.From ||
                change.Before is null)
            {
                continue;
            }

            var pieceRemainsOnBoard =
                execution.Transition.Changes.Any(destination =>
                    ReferenceEquals(destination.After, change.Before));

            if (!pieceRemainsOnBoard)
            {
                return true;
            }
        }

        return false;
    }
}
