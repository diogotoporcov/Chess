// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;

namespace Chess.Variants.Standard.Games.History;

public readonly record struct StandardPiecePlacement(
    Square Square,
    string SideId,
    string PieceDefinitionId);
