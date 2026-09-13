using System.Diagnostics.CodeAnalysis;
using Chess.Core.Board;
using Chess.Core.Games;
using Chess.Core.Movement;
using Chess.Core.Pieces;
using Chess.Core.Sides;
using Chess.Variants.Standard.Board.Regions;
using Chess.Variants.Standard.Movement;
using Chess.Variants.Standard.Pieces;
using Chess.Variants.Standard.Sides;

namespace Chess.Variants.Standard.Games.Rules;

internal static class EnPassantRules
{
    public static IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        if (!gameState.BoardState.TryGetPiece(from, out var pawn))
        {
            yield break;
        }

        if (pawn.Side != gameState.CurrentSide ||
            pawn.Definition.Id != PieceDefinitions.Pawn.Id)
        {
            yield break;
        }

        foreach (var destinationDirection in
                 GetDestinationDirections(pawn.Side))
        {
            if (!gameState.BoardState.Topology.TryGetNext(
                    from,
                    destinationDirection,
                    out var destination))
            {
                continue;
            }

            var move = new Move(from, destination, MoveOptions.EnPassant);

            if (TryGetCapturedPawn(gameState, move, out _, out _))
            {
                yield return move;
            }
        }
    }

    public static bool TryGetCapturedPawn(
        GameState gameState,
        Move move,
        out Square capturedPawnSquare,
        [NotNullWhen(true)] out Piece? capturedPawn)
    {
        ArgumentNullException.ThrowIfNull(gameState);

        capturedPawnSquare = default;
        capturedPawn = null;

        if (move.OptionId != MoveOptions.EnPassant)
        {
            return false;
        }

        var boardState = gameState.BoardState;

        if (!boardState.TryGetPiece(move.From, out var movingPawn))
        {
            return false;
        }

        if (movingPawn.Definition.Id != PieceDefinitions.Pawn.Id)
        {
            return false;
        }

        if (boardState.IsOccupied(move.To))
        {
            return false;
        }

        if (!TryGetAdjacentCaptureSquare(
                gameState,
                movingPawn.Side,
                move.From,
                move.To,
                out capturedPawnSquare))
        {
            return false;
        }

        if (!boardState.TryGetPiece(capturedPawnSquare, out capturedPawn))
        {
            return false;
        }

        if (capturedPawn.Side == movingPawn.Side ||
            capturedPawn.Definition.Id != PieceDefinitions.Pawn.Id)
        {
            capturedPawn = null;
            return false;
        }

        if (!WasLastMoveDoublePawnAdvance(
                gameState,
                capturedPawnSquare,
                capturedPawn))
        {
            capturedPawn = null;
            return false;
        }

        return true;
    }

    private static bool TryGetAdjacentCaptureSquare(
        GameState gameState,
        Side movingSide,
        Square from,
        Square destination,
        out Square adjacentSquare)
    {
        var topology = gameState.BoardState.Topology;

        foreach (var (horizontalDirection, destinationDirection) in
                 GetCaptureDirections(movingSide))
        {
            if (!topology.TryGetNext(
                    from,
                    destinationDirection,
                    out var expectedDestination) ||
                expectedDestination != destination)
            {
                continue;
            }

            if (!topology.TryGetNext(
                    from,
                    horizontalDirection,
                    out adjacentSquare))
            {
                continue;
            }

            return true;
        }

        adjacentSquare = default;

        return false;
    }

    private static bool WasLastMoveDoublePawnAdvance(
        GameState gameState,
        Square currentSquare,
        Piece pawn)
    {
        var lastMove = gameState.LastMove;

        if (lastMove is null)
        {
            return false;
        }

        if (lastMove.Side != pawn.Side)
        {
            return false;
        }

        var move = lastMove.Execution.Move;

        if (move.OptionId is not null ||
            move.To != currentSquare)
        {
            return false;
        }

        if (!gameState.MovementContext.IsInRegion(
                pawn.Side,
                BoardRegions.PawnStarting,
                move.From))
        {
            return false;
        }

        var forwardDirection = GetForwardDirection(pawn.Side);

        var topology = gameState.BoardState.Topology;

        if (!topology.TryGetNext(
                move.From,
                forwardDirection,
                out var intermediateSquare))
        {
            return false;
        }

        if (!topology.TryGetNext(
                intermediateSquare,
                forwardDirection,
                out var expectedDestination))
        {
            return false;
        }

        if (expectedDestination != currentSquare)
        {
            return false;
        }

        var originChange =
            lastMove.Execution.Transition.Changes.SingleOrDefault(change =>
                change.Square == move.From);

        var destinationChange =
            lastMove.Execution.Transition.Changes.SingleOrDefault(change =>
                change.Square == move.To);

        if (originChange is null ||
            destinationChange is null)
        {
            return false;
        }

        return ReferenceEquals(originChange.Before, pawn) &&
               originChange.After is null &&
               destinationChange.Before is null &&
               ReferenceEquals(destinationChange.After, pawn);
    }

    private static IEnumerable<Direction> GetDestinationDirections(
        Side side)
    {
        if (side == SideDefinitions.White)
        {
            yield return CompassDirections.NorthWest;
            yield return CompassDirections.NorthEast;
            yield break;
        }

        if (side == SideDefinitions.Black)
        {
            yield return CompassDirections.SouthWest;
            yield return CompassDirections.SouthEast;
            yield break;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }

    private static IEnumerable<( Direction Horizontal, Direction Destination)>
        GetCaptureDirections(
            Side side)
    {
        if (side == SideDefinitions.White)
        {
            yield return (CompassDirections.West, CompassDirections.NorthWest);

            yield return (CompassDirections.East, CompassDirections.NorthEast);

            yield break;
        }

        if (side == SideDefinitions.Black)
        {
            yield return (CompassDirections.West, CompassDirections.SouthWest);

            yield return (CompassDirections.East, CompassDirections.SouthEast);

            yield break;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }

    private static Direction GetForwardDirection(
        Side side)
    {
        if (side == SideDefinitions.White)
        {
            return CompassDirections.North;
        }

        if (side == SideDefinitions.Black)
        {
            return CompassDirections.South;
        }

        throw new InvalidOperationException(
            $"Unsupported side '{side}' for standard chess.");
    }
}
