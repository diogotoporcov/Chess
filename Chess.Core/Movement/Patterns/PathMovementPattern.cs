// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class PathMovementPattern : IMovementPattern
{
    private readonly DirectionReference[] _path;
    private readonly MovementTargetMode _targetMode;

    public PathMovementPattern(
        params DirectionReference[] path) : this(
        MovementTargetMode.MoveOrCapture,
        path)
    {
    }

    public PathMovementPattern(
        MovementTargetMode targetMode,
        params DirectionReference[] path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (path.Length == 0)
        {
            throw new ArgumentException(
                "Path must contain at least one direction.",
                nameof(path));
        }

        if (!Enum.IsDefined(targetMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(targetMode),
                targetMode,
                "Unsupported movement target mode.");
        }

        _path = [.. path];

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
        var current = from;

        for (var index = 0; index < _path.Length; index++)
        {
            var direction = _path[index]
                .Resolve(context, movingSide);

            if (!boardState.Topology.TryGetNext(
                    current,
                    direction,
                    out var next))
            {
                yield break;
            }

            var isDestination = index == _path.Length - 1;

            if (!isDestination)
            {
                if (boardState.IsOccupied(next))
                {
                    yield break;
                }

                current = next;
                continue;
            }

            if (!boardState.TryGetPiece(next, out var occupyingPiece))
            {
                if (_targetMode is MovementTargetMode.MoveOrCapture
                    or MovementTargetMode.MoveOnly)
                {
                    yield return new Move(from, next);
                }

                yield break;
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
        var current = from;

        for (var index = 0; index < _path.Length; index++)
        {
            var direction = _path[index]
                .Resolve(context, attackingSide);

            if (!boardState.Topology.TryGetNext(
                    current,
                    direction,
                    out var next))
            {
                yield break;
            }

            var isDestination = index == _path.Length - 1;

            if (isDestination)
            {
                yield return next;
                yield break;
            }

            if (boardState.IsOccupied(next))
            {
                yield break;
            }

            current = next;
        }
    }
}
