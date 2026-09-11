using Chess.Core.Board;
using Chess.Core.Movement;

namespace Chess.Core.Games;

public interface IGameMoveGenerator
{
    IEnumerable<Move> GenerateMoves(
        GameState gameState,
        Square from);
}