namespace Chess.Core.Pieces;

public sealed class Piece(PieceColor color)
{
    public PieceColor Color { get; } = color;
}