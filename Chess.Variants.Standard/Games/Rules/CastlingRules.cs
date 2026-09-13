// SPDX-FileCopyrightText: 2026 Diogo Losacco Toporcov
// SPDX-License-Identifier: GPL-3.0-or-later

using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board;
using Chess.Variants.Standard.Games.History;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games.Rules;

internal static class CastlingRules
{
    public static IEnumerable<Move> GenerateCandidates(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(from, out var king))
        {
            yield break;
        }

        if (king.Side != gameState.CurrentSide ||
            king.Definition.Id != PieceDefinitions.King.Id)
        {
            yield break;
        }

        foreach (var optionId in GetOptions())
        {
            var plan = CreatePlan(king.Side, optionId);

            if (from != plan.KingFrom)
            {
                continue;
            }

            var move = new Move(plan.KingFrom, plan.KingTo, optionId);

            if (TryValidateStructure(gameState, move, out _))
            {
                yield return move;
            }
        }
    }

    public static bool TryValidateStructure(
        GameState gameState,
        Move move,
        out CastlingPlan plan)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        plan = default;

        if (move.OptionId != MoveOptions.CastleKingSide &&
            move.OptionId != MoveOptions.CastleQueenSide)
        {
            return false;
        }

        if (!gameState.BoardState.TryGetPiece(move.From, out var king))
        {
            return false;
        }

        if (king.Side != gameState.CurrentSide ||
            king.Definition.Id != PieceDefinitions.King.Id)
        {
            return false;
        }

        var candidate = CreatePlan(king.Side, move.OptionId);

        if (move.From != candidate.KingFrom ||
            move.To != candidate.KingTo)
        {
            return false;
        }

        if (!gameState.BoardState.TryGetPiece(candidate.RookFrom, out var rook))
        {
            return false;
        }

        if (rook.Side != king.Side ||
            rook.Definition.Id != PieceDefinitions.Rook.Id)
        {
            return false;
        }

        var kingSide = move.OptionId == MoveOptions.CastleKingSide;

        if (!CastlingRightsEvaluator.HasRight(gameState, king.Side, kingSide))
        {
            return false;
        }

        foreach (var square in candidate.RequiredEmptySquares)
        {
            if (gameState.BoardState.IsOccupied(square))
            {
                return false;
            }
        }

        plan = candidate with { King = king, Rook = rook };

        return true;
    }

    private static CastlingPlan CreatePlan(
        Side side,
        MoveOptionId optionId)
    {
        var row = GetHomeRow(side);

        var kingFrom = BoardGeometry.SquareAt(row, 4);

        if (optionId == MoveOptions.CastleKingSide)
        {
            var kingThrough = BoardGeometry.SquareAt(row, 5);

            var kingTo = BoardGeometry.SquareAt(row, 6);

            var rookFrom = BoardGeometry.SquareAt(row, 7);

            var rookTo = BoardGeometry.SquareAt(row, 5);

            return new CastlingPlan(
                kingFrom,
                kingThrough,
                kingTo,
                rookFrom,
                rookTo,
                [kingThrough, kingTo]);
        }

        if (optionId == MoveOptions.CastleQueenSide)
        {
            var kingThrough = BoardGeometry.SquareAt(row, 3);

            var kingTo = BoardGeometry.SquareAt(row, 2);

            var rookFrom = BoardGeometry.SquareAt(row, 0);

            var rookTo = BoardGeometry.SquareAt(row, 3);

            return new CastlingPlan(
                kingFrom,
                kingThrough,
                kingTo,
                rookFrom,
                rookTo,
                [BoardGeometry.SquareAt(row, 1), kingTo, kingThrough]);
        }

        throw new ArgumentException(
            $"Move option '{optionId}' is not a castling option.",
            nameof(optionId));
    }

    private static IEnumerable<MoveOptionId> GetOptions()
    {
        yield return MoveOptions.CastleKingSide;

        yield return MoveOptions.CastleQueenSide;
    }

    private static int GetHomeRow(
        Side side)
    {
        if (side == SideDefinitions.White)
        {
            return 7;
        }

        if (side == SideDefinitions.Black)
        {
            return 0;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }
}

internal readonly record struct CastlingPlan(
    Square KingFrom,
    Square KingThrough,
    Square KingTo,
    Square RookFrom,
    Square RookTo,
    IReadOnlyList<Square> RequiredEmptySquares)
{
    public Piece? King { get; init; }

    public Piece? Rook { get; init; }
}
