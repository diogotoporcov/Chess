// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Movement;

namespace Chess.Core.Games;

public interface IGameMoveGenerator
{
    IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from);
}
