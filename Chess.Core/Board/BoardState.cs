using System.Diagnostics.CodeAnalysis;
using Chess.Core.Board.Topology;
using Chess.Core.Pieces;

namespace Chess.Core.Board;

public sealed class BoardState
{
    private readonly Dictionary<Square, Piece> _pieces = [];

    public BoardTopology Topology { get; }

    public BoardState(BoardTopology topology)
    {
        ArgumentNullException.ThrowIfNull(topology);

        Topology = topology;
    }

    public bool IsOccupied(Square square)
    {
        EnsureSquareExists(square);

        return _pieces.ContainsKey(square);
    }

    public bool TryGetPiece(
        Square square,
        [NotNullWhen(true)] out Piece? piece)
    {
        EnsureSquareExists(square);

        return _pieces.TryGetValue(
            square,
            out piece);
    }

    public void PlacePiece(
        Square square,
        Piece piece)
    {
        EnsureSquareExists(square);
        ArgumentNullException.ThrowIfNull(piece);

        if (!_pieces.TryAdd(square, piece))
        {
            throw new InvalidOperationException(
                "Square is already occupied.");
        }
    }

    public Piece RemovePiece(Square square)
    {
        EnsureSquareExists(square);

        if (!_pieces.Remove(square, out var piece))
        {
            throw new InvalidOperationException(
                "Square is not occupied.");
        }

        return piece;
    }

    private void EnsureSquareExists(Square square)
    {
        if (!Topology.Contains(square))
        {
            throw new ArgumentException(
                "Square is not part of the board.",
                nameof(square));
        }
    }
}