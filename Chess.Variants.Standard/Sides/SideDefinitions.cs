// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Sides;

namespace Chess.Variants.Standard.Sides;

public static class SideDefinitions
{
    public static readonly Side White = new("chess:white");
    public static readonly Side Black = new("chess:black");
}
