// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Status;
using Chess.Core.Sides;

namespace Chess.Variants.Standard.Games.Rules;

public sealed class StatusEvaluator : IGameStatusEvaluator
{
    private readonly IGameMoveGenerator _legalMoveGenerator;

    private readonly CheckDetector _checkDetector;

    public StatusEvaluator(
        IGameMoveGenerator legalMoveGenerator,
        CheckDetector checkDetector)
    {
        ArgumentNullException.ThrowIfNull(legalMoveGenerator);
        ArgumentNullException.ThrowIfNull(checkDetector);

        _legalMoveGenerator = legalMoveGenerator;

        _checkDetector = checkDetector;
    }

    public GameStatus Evaluate(
        GameState gameState)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var currentSide = gameState.CurrentSide;

        var isInCheck = _checkDetector.IsInCheck(gameState, currentSide);

        var hasLegalMove = HasLegalMove(gameState, currentSide);

        if (hasLegalMove)
        {
            return new GameStatus(
                isInCheck ? StatusDefinitions.Check : StatusDefinitions.Active,
                isTerminal: false);
        }

        if (!isInCheck)
        {
            return new GameStatus(
                StatusDefinitions.Stalemate,
                isTerminal: true);
        }

        var winner = FindWinningSide(gameState, currentSide);

        return new GameStatus(
            StatusDefinitions.Checkmate,
            isTerminal: true,
            winner);
    }

    private bool HasLegalMove(
        GameState gameState,
        Side side)
    {
        foreach (var position in gameState.BoardState.GetPiecePositions(side))
        {
            if (_legalMoveGenerator
                .GenerateMoves(gameState, position.Square)
                .Any())
            {
                return true;
            }
        }

        return false;
    }

    private static Side FindWinningSide(
        GameState gameState,
        Side losingSide)
    {
        var opponents = gameState
            .TurnOrder
            .Sides
            .Where(side => side != losingSide)
            .ToArray();

        if (opponents.Length != 1)
        {
            throw new InvalidOperationException(
                "Standard chess requires exactly one opposing side.");
        }

        return opponents[0];
    }
}
