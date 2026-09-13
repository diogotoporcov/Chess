// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games;

public static class TurnOrderDefinition
{
    public static TurnOrder Instance { get; } = new(
        SideDefinitions.White,
        SideDefinitions.Black);
}
