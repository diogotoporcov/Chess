using Chess.Core.Board;
using Chess.Core.Movement.Conditions;
using Chess.Core.Sides;

namespace Chess.Core.Movement.Patterns;

public sealed class ConditionalMovementPattern :
    IMovementPattern
{
    private readonly IMovementCondition _condition;

    private readonly IMovementPattern
        _movementPattern;

    public ConditionalMovementPattern(
        IMovementCondition condition,
        IMovementPattern movementPattern)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(movementPattern);

        _condition = condition;
        _movementPattern = movementPattern;
    }

    public IEnumerable<Move> GeneratePseudoLegalMoves(
        MovementContext context,
        Square from,
        Side movingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(movingSide);

        if (!_condition.IsSatisfied(
                context,
                from,
                movingSide))
        {
            yield break;
        }

        foreach (var move in _movementPattern
                     .GeneratePseudoLegalMoves(
                         context,
                         from,
                         movingSide))
        {
            yield return move;
        }
    }

    public IEnumerable<Square> GenerateAttackedSquares(
        MovementContext context,
        Square from,
        Side attackingSide)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(attackingSide);

        if (!_condition.IsSatisfied(
                context,
                from,
                attackingSide))
        {
            yield break;
        }

        foreach (var square in _movementPattern
                     .GenerateAttackedSquares(
                         context,
                         from,
                         attackingSide))
        {
            yield return square;
        }
    }
}