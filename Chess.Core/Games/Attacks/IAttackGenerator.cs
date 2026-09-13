// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Games.Attacks;

public interface IAttackGenerator
{
    IEnumerable<Square> GenerateAttackedSquares(
        GameState gameState,
        Side attackingSide);

    bool IsSquareAttacked(
        GameState gameState,
        Square square,
        Side attackingSide);
}
