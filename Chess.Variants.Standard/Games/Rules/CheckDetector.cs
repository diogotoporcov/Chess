using Chess.Core.Games;
using Chess.Core.Games.Attacks;
using Chess.Core.Sides;

namespace Chess.Variants.Standard.Games.Rules;

public sealed class CheckDetector
{
    private readonly IAttackGenerator _attackGenerator;

    public CheckDetector(
        IAttackGenerator attackGenerator)
    {
        ArgumentNullException.ThrowIfNull(attackGenerator);

        _attackGenerator = attackGenerator;
    }

    public bool IsInCheck(
        GameState gameState,
        Side side)
    {
        ArgumentNullException.ThrowIfNull(gameState);
        ArgumentNullException.ThrowIfNull(side);

        if (!gameState.TurnOrder.Contains(side))
        {
            throw new ArgumentException(
                "Side is not part of the game.",
                nameof(side));
        }

        var kingSquare = KingRules.FindKingSquare(gameState, side);

        foreach (var attackingSide in gameState.TurnOrder.Sides)
        {
            if (attackingSide == side)
            {
                continue;
            }

            if (_attackGenerator.IsSquareAttacked(
                    gameState,
                    kingSquare,
                    attackingSide))
            {
                return true;
            }
        }

        return false;
    }
}
