// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Games.Status;

public interface IGameStatusEvaluator
{
    GameStatus Evaluate(
        GameState gameState);
}
