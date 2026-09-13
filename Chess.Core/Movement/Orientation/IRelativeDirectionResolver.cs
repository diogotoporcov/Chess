// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Orientation;

public interface IRelativeDirectionResolver
{
    Direction Resolve(
        Side side,
        RelativeDirection relativeDirection);
}
