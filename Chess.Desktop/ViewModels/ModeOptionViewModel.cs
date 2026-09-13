// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Windows.Input;
using Chess.Desktop.GameModes;
using Chess.Desktop.Infrastructure.Commands;

namespace Chess.Desktop.ViewModels;

public sealed class ModeOptionViewModel
{
    private readonly GameModeDefinition _mode;

    public string Name => _mode.Variant.Name;

    public string Description => _mode.Description;

    public string PlayerSummary { get; }

    public string BoardSummary => _mode.Presentation.Board.Summary;

    public ICommand SelectCommand { get; }

    public ModeOptionViewModel(
        GameModeDefinition mode,
        Action<GameModeDefinition> selectMode)
    {
        ArgumentNullException.ThrowIfNull(mode);
        ArgumentNullException.ThrowIfNull(selectMode);

        _mode = mode;

        var playerCount = mode.Variant.TurnOrder.Sides.Count;

        PlayerSummary =
            playerCount == 1 ? "1 player" : $"{playerCount} players";

        SelectCommand = new RelayCommand(() => selectMode(_mode));
    }
}
