// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.ObjectModel;
using Chess.Core.Games.Variants;

namespace Chess.Desktop.GameModes;

public sealed class GameModeCatalog
{
    private readonly ReadOnlyCollection<GameModeDefinition> _modes;

    public IReadOnlyList<GameModeDefinition> Modes => _modes;

    public GameModeCatalog(
        IEnumerable<IGameModeProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);

        var modes = providers
            .SelectMany(provider =>
            {
                ArgumentNullException.ThrowIfNull(provider);
                return provider.GetModes();
            })
            .ToArray();

        if (modes.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one game mode must be registered.");
        }

        var duplicate = modes
            .GroupBy(mode => mode.Variant.Id)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new InvalidOperationException(
                $"Multiple game modes are registered with id '{duplicate.Key}'.");
        }

        _modes = Array.AsReadOnly(modes);
    }

    public GameModeDefinition Get(
        GameVariantId variantId)
    {
        ArgumentNullException.ThrowIfNull(variantId);

        return _modes.FirstOrDefault(mode => mode.Variant.Id == variantId) ??
               throw new KeyNotFoundException(
                   $"No game mode is registered with id '{variantId}'.");
    }
}
