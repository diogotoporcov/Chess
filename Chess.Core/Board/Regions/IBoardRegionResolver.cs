// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Sides;

namespace Chess.Core.Board.Regions;

public interface IBoardRegionResolver
{
    bool Contains(
        Side side,
        BoardRegionId regionId,
        Square square);
}
