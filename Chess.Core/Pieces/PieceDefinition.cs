// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Movement.Patterns;

namespace Chess.Core.Pieces;

public sealed class PieceDefinition
{
    public PieceDefinitionId Id { get; }
    public string Name { get; }

    public IReadOnlyList<IMovementPattern> MovementPatterns { get; }

    public PieceDefinition(
        PieceDefinitionId id,
        string name,
        params IMovementPattern?[] movementPatterns)
    {
        ArgumentNullException.ThrowIfNull(id);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Piece name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(movementPatterns);

        var validatedPatterns =
            new IMovementPattern[movementPatterns.Length];

        for (var index = 0; index < movementPatterns.Length; index++)
        {
            var pattern = movementPatterns[index];

            if (pattern is null)
            {
                throw new ArgumentException(
                    "Movement patterns cannot contain null entries.",
                    nameof(movementPatterns));
            }

            validatedPatterns[index] = pattern;
        }

        Id = id;
        Name = name.Trim();

        MovementPatterns = Array.AsReadOnly(validatedPatterns);
    }
}
