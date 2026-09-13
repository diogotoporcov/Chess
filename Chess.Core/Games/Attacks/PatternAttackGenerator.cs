// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Games.Attacks;

public sealed class PatternAttackGenerator : IAttackGenerator
{
    public IEnumerable<Square> GenerateAttackedSquares(
        GameState gameState,
        Side attackingSide)
    {
        Validate(gameState, attackingSide);

        var attackedSquares = new HashSet<Square>();

        foreach (var position in gameState.BoardState.GetPiecePositions(
                     attackingSide))
        {
            foreach (var square in position.Piece.GenerateAttackedSquares(
                         gameState.MovementContext,
                         position.Square))
            {
                if (attackedSquares.Add(square))
                {
                    yield return square;
                }
            }
        }
    }

    public bool IsSquareAttacked(
        GameState gameState,
        Square square,
        Side attackingSide)
    {
        Validate(gameState, attackingSide);

        if (!gameState.BoardState.Topology.Contains(square))
        {
            throw new ArgumentException(
                "Square is not part of the board.",
                nameof(square));
        }

        foreach (var position in gameState.BoardState.GetPiecePositions(
                     attackingSide))
        {
            if (position
                .Piece
                .GenerateAttackedSquares(
                    gameState.MovementContext,
                    position.Square)
                .Contains(square))
            {
                return true;
            }
        }

        return false;
    }

    private static void Validate(
        GameState gameState,
        Side attackingSide)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(attackingSide);

        if (!gameState.TurnOrder.Contains(attackingSide))
        {
            throw new ArgumentException(
                "Attacking side is not part of the game.",
                nameof(attackingSide));
        }
    }
}
