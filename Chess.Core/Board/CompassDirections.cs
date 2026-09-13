// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Board;

public static class CompassDirections
{
    public static readonly Direction North = new("North");
    public static readonly Direction NorthEast = new("NorthEast");
    public static readonly Direction East = new("East");
    public static readonly Direction SouthEast = new("SouthEast");
    public static readonly Direction South = new("South");
    public static readonly Direction SouthWest = new("SouthWest");
    public static readonly Direction West = new("West");
    public static readonly Direction NorthWest = new("NorthWest");
}
