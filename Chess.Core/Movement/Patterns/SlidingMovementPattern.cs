// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class SlidingMovementPattern : IMovementPattern
{
    private readonly DirectionReference _direction;
    private readonly int? _maxDistance;
    private readonly MovementTargetMode _targetMode;

    public SlidingMovementPattern(
        DirectionReference direction,
        int? maxDistance = null,
        MovementTargetMode targetMode = MovementTargetMode.MoveOrCapture)
    {
        ArgumentNullException.ThrowIfNull(direction);

        if (maxDistance is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxDistance),
                "Maximum distance must be greater than zero.");
        }

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetMode),
                targetMode,
                "Unsupported movement target mode.");
        }

        _direction = direction;
        _maxDistance = maxDistance;
        _targetMode = targetMode;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(movingSide);

        var boardState = context.BoardState;

        var direction = _direction.Resolve(context, movingSide);

        var current = from;
        var distance = 0;

        while ((!_maxDistance.HasValue || distance < _maxDistance.Value) &&
               boardState.Topology.TryGetNext(current, direction, out var next))
        {
            distance++;

            if (!boardState.TryGetPiece(next, out var occupyingPiece))
            {
                if (_targetMode is MovementTargetMode.MoveOrCapture
                    or MovementTargetMode.MoveOnly)
                {
                    yield return new Move(from, next);
                }

                current = next;
                continue;
            }

            if (occupyingPiece.Side != movingSide &&
                _targetMode is MovementTargetMode.MoveOrCapture
                    or MovementTargetMode.CaptureOnly)
            {
                yield return new Move(from, next);
            }

            yield break;
        }
    }

    public IEnumerable<Square> GenerateAttackedSquares(
        MovementContext context,
        Square from,
        Side attackingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(attackingSide);

        if (_targetMode == MovementTargetMode.MoveOnly)
        {
            yield break;
        }

        var boardState = context.BoardState;

        var direction = _direction.Resolve(context, attackingSide);

        var current = from;
        var distance = 0;

        while ((!_maxDistance.HasValue || distance < _maxDistance.Value) &&
               boardState.Topology.TryGetNext(current, direction, out var next))
        {
            distance++;

            yield return next;

            if (boardState.IsOccupied(next))
            {
                yield break;
            }

            current = next;
        }
    }
}
