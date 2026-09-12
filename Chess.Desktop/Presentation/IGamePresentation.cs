using Chess.Core.Games.Status;
using Chess.Core.Pieces;
using Chess.Core.Sides;

namespace Chess.Desktop.Presentation;

public interface IGamePresentation
{
    BoardPresentation Board { get; }

    string GetSideName(
        Side side);

    string GetStatusName(
        GameStatusId statusId);

    string GetPieceImageSource(
        Piece piece);
}
