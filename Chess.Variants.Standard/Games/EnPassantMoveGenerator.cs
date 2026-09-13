// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Variants.Standard.Games.Rules;

namespace Chess.Variants.Standard.Games;

public sealed class EnPassantMoveGenerator : IGameMoveGenerator
{
    private readonly IGameMoveGenerator _innerMoveGenerator;

    public EnPassantMoveGenerator(
        IGameMoveGenerator innerMoveGenerator)
    {
        ArgumentNullException.ThrowIfNull(innerMoveGenerator);

        _innerMoveGenerator = innerMoveGenerator;
    }

    public IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        foreach (var move in _innerMoveGenerator.GenerateMoves(gameState, from))
        {
            yield return move;
        }

        foreach (var move in EnPassantRules.GenerateMoves(gameState, from))
        {
            yield return move;
        }
    }
}
