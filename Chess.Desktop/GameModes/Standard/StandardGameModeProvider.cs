// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Desktop.Presentation.Standard;
using Chess.Variants.Standard;

namespace Chess.Desktop.GameModes.Standard;

public sealed class StandardGameModeProvider : IGameModeProvider
{
    private static readonly GameModeDefinition Mode = new(
        Variant.Definition,
        "Classic chess with the standard rules.",
        new StandardChessPresentation());

    public IEnumerable<GameModeDefinition> GetModes()
    {
        yield return Mode;
    }
}
