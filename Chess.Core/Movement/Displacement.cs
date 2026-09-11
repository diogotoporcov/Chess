namespace Chess.Core.Movement;

public sealed class Displacement : IEquatable<Displacement>
{
    private readonly DisplacementComponent[] _components;
    
    public IReadOnlyList<DisplacementComponent> Components => _components;

    public Displacement(
        params DisplacementComponent[] components)
    {
        ArgumentNullException.ThrowIfNull(components);

        if (components.Length == 0)
        {
            throw new ArgumentException(
                "Displacement must contain at least one component.",
                nameof(components)
            );
        }

        if (components.Any(component => component.Distance <= 0))
        {
            throw new ArgumentException(
                "Displacement distance must be greater than zero.",
                nameof(components)
            );
        }

        if (components
            .GroupBy(component => component.Direction)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Displacement cannot contain multiple components of the same direction.",
                nameof(components)
            );
        }

        _components =
        [
            .. components.OrderBy(
                component => component.Direction.Name,
                StringComparer.Ordinal)
        ];
    }

    public bool Equals(Displacement? other)
    {
        return other is not null && 
               _components.SequenceEqual(other._components);
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