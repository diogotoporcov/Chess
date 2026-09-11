using System.Collections.Frozen;
using Chess.Core.Sides;

namespace Chess.Core.Board.Regions;

public sealed class SideBoardRegionMap : IBoardRegionResolver
{
    private readonly FrozenDictionary<
        (Side Side, BoardRegionId RegionId),
        BoardRegion> _regions;

    public SideBoardRegionMap(
        params (
            Side Side,
            BoardRegionId RegionId,
            BoardRegion Region)[] mappings)
    {
        ArgumentNullException.ThrowIfNull(mappings);

        if (mappings.Length == 0)
        {
            throw new ArgumentException(
                "At least one board region mapping is required.",
                nameof(mappings));
        }

        var regions = new Dictionary<
            (Side Side, BoardRegionId RegionId),
            BoardRegion>();

        foreach (var mapping in mappings)
        {
            ArgumentNullException.ThrowIfNull(mapping.Side);
            ArgumentNullException.ThrowIfNull(mapping.RegionId);
            ArgumentNullException.ThrowIfNull(mapping.Region);

            if (!regions.TryAdd(
                    (mapping.Side, mapping.RegionId),
                    mapping.Region))
            {
                throw new ArgumentException(
                    "A board region mapping already exists for this side and region id.",
                    nameof(mappings));
            }
        }

        _regions = regions.ToFrozenDictionary();
    }

    public bool Contains(
        Side side,
        BoardRegionId regionId,
        Square square)
    {
        ArgumentNullException.ThrowIfNull(side);
        ArgumentNullException.ThrowIfNull(regionId);

        if (!_regions.TryGetValue(
                (side, regionId),
                out var region))
        {
            throw new InvalidOperationException(
                $"No board region mapping exists for side '{side}' " +
                $"and region '{regionId}'.");
        }

        return region.Contains(square);
    }
}