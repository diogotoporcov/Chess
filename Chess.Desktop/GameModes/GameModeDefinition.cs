// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;
using Chess.Core.Games.Variants;
using Chess.Desktop.Presentation;

namespace Chess.Desktop.GameModes;

public sealed class GameModeDefinition
{
    public GameVariantDefinition Variant { get; }

    public string Description { get; }

    public IGamePresentation Presentation { get; }

    public GameModeDefinition(
        GameVariantDefinition variant,
        string description,
        IGamePresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(variant);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Game mode description cannot be empty.",
                nameof(description));
        }

        ArgumentNullException.ThrowIfNull(presentation);

        Variant = variant;
        Description = description.Trim();
        Presentation = presentation;
    }

    public Game CreateGame()
    {
        return Variant.CreateGame();
    }
}
