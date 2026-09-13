// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Movement.Orientation;

public sealed record RelativeDirection
{
    public string Name { get; }

    public RelativeDirection(
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Relative direction name cannot be empty.",
                nameof(name));
        }

        Name = name.Trim();
    }

    public override string ToString()
    {
        return Name;
    }
}
