// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.ObjectModel;
using Chess.Core.Board;

namespace Chess.Variants.Standard.Games.History;

public sealed class StandardPositionKey : IEquatable<StandardPositionKey>
{
    private readonly StandardPiecePlacement[] _piecePlacements;

    private readonly ReadOnlyCollection<StandardPiecePlacement>
        _piecePlacementsView;

    public IReadOnlyList<StandardPiecePlacement> PiecePlacements =>
        _piecePlacementsView;

    public string SideToMoveId { get; }

    public CastlingRights CastlingRights { get; }

    public Square? EffectiveEnPassantTarget { get; }

    internal StandardPositionKey(
        IEnumerable<StandardPiecePlacement> piecePlacements,
        string sideToMoveId,
        CastlingRights castlingRights,
        Square? effectiveEnPassantTarget)
    {
        ArgumentNullException.ThrowIfNull(piecePlacements);

        if (string.IsNullOrWhiteSpace(sideToMoveId))
        {
            throw new ArgumentException(
                "Side-to-move id cannot be empty.",
                nameof(sideToMoveId));
        }

        var canonicalPlacements = piecePlacements
            .OrderBy(placement => placement.Square.Id)
            .ToArray();

        if (canonicalPlacements
            .GroupBy(placement => placement.Square)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException(
                "Piece placements cannot contain multiple pieces on the same square.",
                nameof(piecePlacements));
        }

        _piecePlacements = canonicalPlacements;
        _piecePlacementsView = Array.AsReadOnly(_piecePlacements);

        SideToMoveId = sideToMoveId;
        CastlingRights = castlingRights;
        EffectiveEnPassantTarget = effectiveEnPassantTarget;
    }

    public bool Equals(
        StandardPositionKey? other)
    {
        return other is not null &&
               StringComparer.Ordinal.Equals(
                   SideToMoveId,
                   other.SideToMoveId) &&
               CastlingRights == other.CastlingRights &&
               EffectiveEnPassantTarget == other.EffectiveEnPassantTarget &&
               _piecePlacements.SequenceEqual(other._piecePlacements);
    }

    public override bool Equals(
        object? obj)
    {
        return obj is StandardPositionKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        const uint offsetBasis = 2166136261;
        var hash = offsetBasis;

        hash = AddString(hash, SideToMoveId);
        hash = Add(hash, CastlingRights.WhiteKingSide);
        hash = Add(hash, CastlingRights.WhiteQueenSide);
        hash = Add(hash, CastlingRights.BlackKingSide);
        hash = Add(hash, CastlingRights.BlackQueenSide);
        hash = Add(hash, EffectiveEnPassantTarget.HasValue);

        if (EffectiveEnPassantTarget is { } enPassantTarget)
        {
            hash = Add(hash, enPassantTarget.Id);
        }

        foreach (var placement in _piecePlacements)
        {
            hash = Add(hash, placement.Square.Id);
            hash = AddString(hash, placement.SideId);
            hash = AddString(hash, placement.PieceDefinitionId);
        }

        return unchecked((int)hash);
    }

    public static bool operator ==(
        StandardPositionKey? left,
        StandardPositionKey? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(
        StandardPositionKey? left,
        StandardPositionKey? right)
    {
        return !Equals(left, right);
    }

    private static uint Add(
        uint hash,
        bool value)
    {
        return Add(hash, value ? 1 : 0);
    }

    private static uint Add(
        uint hash,
        int value)
    {
        const uint prime = 16777619;

        return unchecked((hash ^ (uint)value) * prime);
    }

    private static uint AddString(
        uint hash,
        string value)
    {
        hash = Add(hash, value.Length);

        foreach (var character in value)
        {
            hash = Add(hash, character);
        }

        return hash;
    }
}
