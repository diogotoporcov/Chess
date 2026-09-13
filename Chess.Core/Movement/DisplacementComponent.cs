// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Movement;

public sealed record DisplacementComponent
{
    public DirectionReference Direction { get; }
    public int Distance { get; }

    public DisplacementComponent(
        DirectionReference direction,
        int distance)
    {
        ArgumentNullException.ThrowIfNull(direction);

        if (distance <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(distance),
                "Displacement distance must be greater than zero.");
        }

        Direction = direction;
        Distance = distance;
    }
}
