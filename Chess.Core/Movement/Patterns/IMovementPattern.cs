// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public interface IMovementPattern
{
    IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from,
        Side movingSide);

    IEnumerable<Square> GenerateAttackedSquares(
        MovementContext context,
        Square from,
        Side attackingSide);
}
