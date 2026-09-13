// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

namespace Chess.Core.Movement;

public sealed class Displacement : IEquatable<Displacement>
{
    private readonly IReadOnlyList<DisplacementComponent> _components;

    public IReadOnlyList<DisplacementComponent> Components => _components;

    public Displacement(
        params DisplacementComponent[] components)
    {
        ArgumentNullException.ThrowIfNull(components);

        if (components.Length == 0)
        {
            throw new ArgumentException(
                "Displacement must contain at least one component.",
                nameof(components));
        }

        if (components
            .GroupBy(component => component.Direction)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Displacement cannot contain multiple components of the same direction.",
                nameof(components));
        }

        var orderedComponents = components
            .OrderBy(
                component => component.Direction.SortKey,
                StringComparer.Ordinal)
            .ToArray();

        _components = Array.AsReadOnly(orderedComponents);
    }

    public bool Equals(
        Displacement? other)
    {
        return other is not null &&
               _components.SequenceEqual(other._components);
    }

    public override bool Equals(
        object? obj)
    {
        return obj is Displacement other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (var component in _components)
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }
}
