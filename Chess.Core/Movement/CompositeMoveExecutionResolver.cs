// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Games;

namespace Chess.Core.Movement;

public sealed class CompositeMoveExecutionResolver : IMoveExecutionResolver
{
    private readonly IReadOnlyList<IMoveExecutionResolver> _resolvers;

    public CompositeMoveExecutionResolver(
        params IMoveExecutionResolver[] resolvers)
    {
        ArgumentNullException.ThrowIfNull(resolvers);

        if (resolvers.Length == 0)
        {
            throw new ArgumentException(
                "At least one move execution resolver is required.",
                nameof(resolvers));
        }

        _resolvers = Array.AsReadOnly([.. resolvers]);
    }

    public bool CanResolve(
        Move move)
    {
        return _resolvers.Any(resolver => resolver.CanResolve(move));
    }

    public MoveExecution Resolve(
        GameState gameState,
        Move move)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        var matchingResolvers = _resolvers
            .Where(resolver => resolver.CanResolve(move))
            .Take(2)
            .ToArray();

        if (matchingResolvers.Length == 0)
        {
            throw new InvalidOperationException(
                $"No move execution resolver can handle move option '{move.OptionId?.ToString() ?? "none"}'.");
        }

        if (matchingResolvers.Length > 1)
        {
            throw new InvalidOperationException(
                $"More than one move execution resolver can handle move option '{move.OptionId?.ToString() ?? "none"}'.");
        }

        return matchingResolvers[0]
            .Resolve(gameState, move);
    }
}
